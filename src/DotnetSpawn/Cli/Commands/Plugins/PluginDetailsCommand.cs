using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DotnetSpawn.IO;
using DotnetSpawn.Plugin;
using DotnetSpawn.Plugins;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using Spectre.Console;
using Spectre.Console.Cli;

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
                var plugins = _pluginLoader.LoadPlugins();

                var plugin = SelectPlugin(plugins, packageIdentity);

                if (plugin == null)
                {
                    _console.LogError($"Unrecognized plugin {packageIdentity}");
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

        private SpawnPointPlugin SelectPlugin(SpawnPointPluginCollection plugins, PackageIdentity packageIdentity)
        {
            if (packageIdentity == null)
            {
                return PromptPlugin(plugins);
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

            var rule = new Rule($"[teal]Spawn points[/]")
                .LeftAligned()
                .RuleStyle("green");

            SpectreConsole.Render(rule);

            _console.WriteLine(string.Empty, IO.Style.None);

            DisplaySpawnPointsTable(spawnPointMetadatas);

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

            var basicInfoTable = new Table()
                .Border(TableBorder.None)
                .HideHeaders()
                .AddColumns(string.Empty, string.Empty)
                .AddRow(new Text("Version:"), new Text(plugin.Version.ToString(), "orange3"))
                .AddRow(new Text("Author(s):"), new Text(plugin.NuspecReader.GetAuthors(), "orange3"));

            var repositoryUrl = plugin.NuspecReader.GetRepositoryMetadata().Url;

            if (!string.IsNullOrEmpty(repositoryUrl))
            {
                basicInfoTable.AddRow(new Text("Repository URL:"), new Text(repositoryUrl, "orange3"));
            }

            basicInfoTable.AddRow(
                new Text("Supported frameworks:"),
                new Text(string.Join(", ", plugin.SupportedFrameworks), "orange3"));

            if (plugin.SelectedFramework != null)
            {
                basicInfoTable.AddRow(
                    new Text("Chosen framework:"),
                    new Text(plugin.SelectedFramework.ToString(), "orange3"));
            }

            if (!plugin.IsCompatible)
            {
                basicInfoTable.AddRow(new Text("Compatible:"), new Text("false", "maroon"));
            }
            else
            {
                basicInfoTable.AddRow(
                    new Text("Main assembly path:"),
                    new Text(plugin.MainAssemblyFile.FullName, "orange3"));
            }

            SpectreConsole.Render(basicInfoTable);

            _console.WriteLine(string.Empty, IO.Style.None);
        }

        private void DisplaySpawnPointsTable(IEnumerable<SpawnPointMetadata> metadatas)
        {
            var spawnPointsTable = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("Spawn point")
                .AddColumn("Description");

            foreach (var metadata in metadatas)
            {
                spawnPointsTable.AddRow(metadata.Fqn.IdAlias, metadata.Description);
            }

            SpectreConsole.Render(spawnPointsTable);

            _console.WriteLine(string.Empty, IO.Style.None);
        }

        private void DisplaySpawnPointInfo(SpawnPointMetadata metadata)
        {
            var rule = new Rule($"[teal]{metadata.Fqn.IdAlias}[/]")
                .LeftAligned();

            SpectreConsole.Render(rule);

            _console.WriteLine(string.Empty, IO.Style.None);

            var aliasTable = new Table()
                .Border(TableBorder.None)
                .HideHeaders()
                .AddColumns(string.Empty, string.Empty)
                .AddRow(new Text("Id:"), new Text(metadata.Fqn.IdAlias, "orange3"))
                .AddRow(new Text("Versioned alias:"), new Text(metadata.Fqn.VersionedAlias, "orange3"))
                .AddRow(new Text("Fully qualified name:"), new Text(metadata.Fqn.Fqn, "orange3"));

            SpectreConsole.Render(aliasTable);

            _console.WriteLine(string.Empty, IO.Style.None);

            if (metadata.HasInputs)
            {
                var inputsTable = new Table()
                    .Border(TableBorder.Rounded)
                    .AddColumns("Input name", "Type", "Required?", "Description");

                foreach (var input in metadata.Inputs)
                {
                    inputsTable.AddRow(
                        new Text(input.Name),
                        new Text(input.Type.Name),
                        new Markup(input.Required ? ":check_mark:" : string.Empty).Centered(),
                        new Text(input.Description));
                }

                SpectreConsole.Render(inputsTable);

                _console.WriteLine(string.Empty, IO.Style.None);
            }

            if (metadata.HasOutput)
            {
                var outputsTable = new Table()
                    .Border(TableBorder.Rounded)
                    .AddColumns("Output name", "Type", "Description");

                foreach (var output in metadata.Outputs)
                {
                    outputsTable.AddRow(
                        new Text(output.Name),
                        new Text(output.Type.Name),
                        new Text(output.Description));
                }

                SpectreConsole.Render(outputsTable);

                _console.WriteLine(string.Empty, IO.Style.None);
            }
        }
    }
}