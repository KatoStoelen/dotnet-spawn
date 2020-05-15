using System.ComponentModel;
using DotnetSpawn.Extensions;
using DotnetSpawn.IO;
using DotnetSpawn.Plugin;
using DotnetSpawn.Plugins;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Rendering;

namespace DotnetSpawn.Cli.Commands.Plugins
{
    [Description("Show details of a plugin")]
    internal class PluginDetailsCommand : Command<PluginDetailsCommand.Settings>
    {
        public class Settings : GlobalSettings
        {
            [CommandArgument(0, "[PLUGIN]")]
            [Description("Name of the plugin to display details of. If not specified, a prompt will be shown")]
            public string PluginName { get; set; }

            [CommandOption("-v|--version <VERSION>")]
            [Description("Version of the plugin to display details of. Only applicable if a plugin name is specified")]
            public string Version { get; set; }

            public PackageIdentity GetPackageIdentity()
            {
                if (string.IsNullOrWhiteSpace(PluginName))
                {
                    return null;
                }

                return new PackageIdentity(
                    PluginName,
                    string.IsNullOrWhiteSpace(Version)
                        ? null
                        : new NuGetVersion(Version));
            }
        }

        private readonly SpawnPointPluginLoader _pluginLoader;
        private readonly SpectreConsole _console;

        public PluginDetailsCommand(SpawnPointPluginLoader pluginLoader, SpectreConsole console)
        {
            _pluginLoader = pluginLoader;
            _console = console;
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            try
            {
                var packageIdentity = settings.GetPackageIdentity();
                var plugins = _pluginLoader.LoadAllPlugins();

                var plugin = SelectPlugin(plugins, packageIdentity);

                if (plugin == null)
                {
                    _console.LogError($"Unknown plugin: {packageIdentity}");
                }

                DisplayPluginDetails(plugin);

                return 0;
            }
            catch (Exception e)
            {
                _console.LogError(e.Message, e);

                return 1;
            }
        }

        private SpawnPointPlugin SelectPlugin(
            SpawnPointPluginCollection plugins, PackageIdentity packageIdentity)
        {
            if (packageIdentity == null)
            {
                return plugins.Count == 1
                    ? plugins.Single()
                    : PromptPlugin(plugins);
            }
            else
            {
                if (packageIdentity.HasVersion)
                {
                    return plugins.SingleOrDefault(plugin => plugin.Equals(packageIdentity));
                }
                else
                {
                    var filteredPlugins = plugins
                        .Where(plugin =>
                            plugin.Name.Equals(
                                packageIdentity.Id,
                                StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (!filteredPlugins.Any())
                    {
                        return null;
                    }

                    return filteredPlugins.Count == 1
                        ? filteredPlugins.Single()
                        : PromptPlugin(filteredPlugins);
                }
            }
        }

        private SpawnPointPlugin PromptPlugin(IEnumerable<SpawnPointPlugin> plugins)
        {
            var options = plugins
                .Select(plugin => new PromptOption<SpawnPointPlugin>(plugin, plugin.ToString()));

            return _console.Prompt(options, "Select plugin to show details of:");
        }

        private void DisplayPluginDetails(SpawnPointPlugin plugin)
        {
            DisplayBasicInfoSection(plugin);

            var spawnPointMetadatas = plugin.GetSpawnPoints()
                .Select(spawnPoint => new SpawnPointMetadata(spawnPoint));

            foreach (var metadata in spawnPointMetadatas)
            {
                DisplaySpawnPointInfo(metadata);
            }
        }

        private void DisplayBasicInfoSection(SpawnPointPlugin plugin)
        {
            var rule = new Rule($"[teal]{plugin.Name}[/]")
                .LeftAligned()
                .RuleStyle("green");

            SpectreConsole.Render(rule);

            _console.WriteLine(string.Empty, IO.Style.None);

            var summary = plugin.NuspecReader.GetSummary();

            if (!string.IsNullOrWhiteSpace(summary))
            {
                _console.WriteLine(summary, IO.Style.None);
                _console.WriteLine(string.Empty, IO.Style.None);
            }

            var basicInfo = new Dictionary<string, string>
            {
                ["Version"] = plugin.Version.ToString(),
                ["Author(s)"] = plugin.NuspecReader.GetAuthors()
            };

            var repositoryUrl = plugin.NuspecReader.GetRepositoryMetadata().Url;

            if (!string.IsNullOrEmpty(repositoryUrl))
            {
                basicInfo.Add("Repository URL", repositoryUrl);
            }

            basicInfo.Add("Supported framework(s)", string.Join(", ", plugin.SupportedFrameworks));

            if (plugin.SelectedFramework != null)
            {
                basicInfo.Add("Chosen framework", plugin.SelectedFramework.ToString());
            }

            if (!plugin.IsCompatible)
            {
                basicInfo.Add("Compatible", "[maroon]false[/]");
            }
            else
            {
                basicInfo.Add("Main assembly path", plugin.MainAssemblyFile.FullName);
            }

            SpectreConsole.RenderKeyValueTable(basicInfo);

            _console.WriteLine(string.Empty, IO.Style.None);
        }

        private void DisplaySpawnPointInfo(SpawnPointMetadata metadata)
        {
            var rule = new Rule($"Spawn point: [teal]{metadata.Fqn.Name}[/]")
                .LeftAligned()
                .RuleStyle("green");

            SpectreConsole.Render(rule);

            _console.WriteLine(string.Empty, IO.Style.None);

            if (!string.IsNullOrWhiteSpace(metadata.Description))
            {
                _console.WriteLine($"[italic]{metadata.Description}[/]", IO.Style.None);
                _console.WriteLine(string.Empty, IO.Style.None);
            }

            var aliases = new Dictionary<string, string>
            {
                ["Id"] = metadata.Fqn.Name,
                ["Versioned alias"] = metadata.Fqn.VersionedName,
                ["Plugin qualified name"] = metadata.Fqn.PluginQualifiedName,
                ["Fully qualified name"] = metadata.Fqn.FullyQualifiedName
            };

            SpectreConsole.RenderKeyValueTable(aliases);

            _console.WriteLine(string.Empty, IO.Style.None);

            if (metadata.HasInputs)
            {
                var inputsTable = new Table()
                    .Border(TableBorder.Rounded)
                    .AddColumns("Name", "Type", "Required?", "Description")
                    .Title("[teal]Inputs[/]")
                    .LeftAligned();

                foreach (var input in metadata.Inputs.NestedInputs)
                {
                    var hasNestedInputs = input.NestedInputs.Any();

                    inputsTable.AddRow(
                        new Text(input.Name),
                        new Text(hasNestedInputs ? string.Empty : input.Type.ToPrettyName()),
                        new Markup(input.Required ? ":check_mark:" : string.Empty).Centered(),
                        new Text(input.Description));

                    if (hasNestedInputs)
                    {
                        var nestedRows = GetNestedInputRows(input.NestedInputs);

                        foreach (var row in nestedRows)
                        {
                            inputsTable.AddRow(row);
                        }
                    }
                }

                SpectreConsole.Render(inputsTable);

                _console.WriteLine(string.Empty, IO.Style.None);
            }

            if (metadata.HasOutput)
            {
                var outputsTable = new Table()
                    .Border(TableBorder.Rounded)
                    .AddColumns("Name", "Type", "Description")
                    .Title("[teal]Outputs[/]");

                foreach (var output in metadata.Outputs)
                {
                    outputsTable.AddRow(
                        new Text(output.Name),
                        new Text(output.Type.ToPrettyName()),
                        new Text(output.Description));
                }

                SpectreConsole.Render(outputsTable);

                _console.WriteLine(string.Empty, IO.Style.None);
            }
        }

        private static IEnumerable<IEnumerable<IRenderable>> GetNestedInputRows(
            IReadOnlyCollection<SpawnPointMetadata.InputMetadata> nestedInputs,
            string parentPrefix = "")
        {
            const string Mid = "\u251C\u2500";
            const string MidEmpty = "\u2502";
            const string End = "\u2514\u2500";

            var index = 0;

            foreach (var input in nestedInputs)
            {
                var isLast = index == nestedInputs.Count - 1;
                var hasNestedInputs = input.NestedInputs.Any();
                var prefix = parentPrefix + (isLast ? End : Mid);

                yield return new IRenderable[]
                {
                    new Text($"{prefix} {input.Name}"),
                    new Text(hasNestedInputs ? string.Empty : input.Type.ToPrettyName()),
                    new Markup(input.Required ? ":check_mark:" : string.Empty).Centered(),
                    new Text(input.Description)
                };

                if (hasNestedInputs)
                {
                    var nestedRows = GetNestedInputRows(
                        input.NestedInputs,
                        parentPrefix + (isLast ? new string(' ', 3) : MidEmpty.PadRight(3, ' ')));

                    foreach (var row in nestedRows)
                    {
                        yield return row;
                    }
                }

                index++;
            }
        }
    }
}