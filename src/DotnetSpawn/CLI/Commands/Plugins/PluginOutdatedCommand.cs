using System.ComponentModel;
using DotnetSpawn.Cli.Commands;
using DotnetSpawn.IO;
using DotnetSpawn.NuGet;
using DotnetSpawn.Plugins;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DotnetSpawn.CLI.Commands.Plugins
{
    [Description("List outdated plugins")]
    internal class PluginOutdatedCommand : CancellableAsyncCommand<PluginOutdatedCommand.Settings>
    {

        public class Settings : NuGetCommonSettings
        {
            [CommandOption("--short")]
            [Description("Display short summary")]
            public bool Short { get; set; }
        }

        private readonly SpawnPointPluginLoader _pluginLoader;
        private readonly NuGetPackageFinder _packageFinder;
        private readonly SpectreConsole _console;

        public PluginOutdatedCommand(
            SpawnPointPluginLoader pluginLoader,
            NuGetPackageFinder packageFinder,
            SpectreConsole console)
        {
            _pluginLoader = pluginLoader;
            _packageFinder = packageFinder;
            _console = console;
        }

        protected override async Task<int> ExecuteAsync(Settings settings, CancellationToken cancellationToken)
        {
            try
            {
                var plugins = _pluginLoader.LoadAllPlugins();
                var outdatedPlugins = await GetOutdatedPluginsAsync(
                    plugins, settings, cancellationToken);

                if (!outdatedPlugins.Any())
                {
                    _console.WriteLine("All plugins up to date.", IO.Style.HighlightAlt);
                }
                else
                {
                    if (settings.Short)
                    {
                        DisplayShortOutdatedSummary(outdatedPlugins);
                    }
                    else
                    {
                        DisplayOutdatedTable(outdatedPlugins, plugins);
                    }
                }

                return 0;
            }
            catch (Exception e)
            {
                _console.LogError(e.Message, e);

                return 1;
            }
        }

        private async Task<Dictionary<SpawnPointPlugin, NuGetVersion>> GetOutdatedPluginsAsync(
            SpawnPointPluginCollection plugins,
            Settings settings,
            CancellationToken cancellationToken)
        {
            var outdatedPlugins = new Dictionary<SpawnPointPlugin, NuGetVersion>(
                new VersionExcludingPluginComparer());

            var pluginsByName = plugins.GroupBy(plugin => plugin.Name);

            foreach (var group in pluginsByName)
            {
                var latestPlugin = group.OrderByDescending(plugin => plugin).First();

                if (outdatedPlugins.ContainsKey(latestPlugin))
                {
                    continue;
                }

                var packageIdentity = new PackageIdentity(
                    latestPlugin.Name, version: null);

                var latestVersion = await FindLatestVersionAsync(
                    packageIdentity, settings, cancellationToken);

                if (latestPlugin.Version >= latestVersion)
                {
                    continue;
                }

                outdatedPlugins.Add(latestPlugin, latestVersion);
            }

            return outdatedPlugins;
        }

        private async Task<NuGetVersion> FindLatestVersionAsync(
            PackageIdentity package,
            Settings settings,
            CancellationToken cancellationToken)
        {
            var findPackageResult = await _packageFinder.FindAsync(
                package,
                settings.GetPackageSource(),
                settings.NuGetConfigFile,
                settings.IncludePrerelease,
                cancellationToken);

            if (!findPackageResult.Found)
            {
                _console.LogWarning($"Failed to determine version of {package}. Not found.");

                return null;
            }

            return findPackageResult.Package.Version;
        }

        private static void DisplayShortOutdatedSummary(
            Dictionary<SpawnPointPlugin, NuGetVersion> outdatedPlugins)
        {
            foreach (var outdatedPlugin in outdatedPlugins.OrderBy(pair => pair.Key.Name))
            {
                var markupRow = new[]
                {
                    outdatedPlugin.Key.Name,
                    $"[yellow]{outdatedPlugin.Key.Version}[/]",
                    $"[green]{outdatedPlugin.Value}[/]"
                };

                SpectreConsole.Render(
                    new Markup(string.Join('|', markupRow)),
                    newLineAfter: true);
            }
        }

        private static void DisplayOutdatedTable(
            Dictionary<SpawnPointPlugin, NuGetVersion> outdatedPlugins,
            SpawnPointPluginCollection allPlugins)
        {
            var pluginsByName = allPlugins
                .GroupBy(plugin => plugin.Name)
                .ToDictionary(
                    group => group.Key,
                    group => group.ToList(),
                    StringComparer.OrdinalIgnoreCase);

            var table = new Table()
                .AddColumns("Plugin", "Current version(s)", "Latest version")
                .Border(TableBorder.Rounded);

            table.Title = new TableTitle("[teal]Outdated plugins[/]");

            foreach (var outdatedPlugin in outdatedPlugins.OrderBy(pair => pair.Key.Name))
            {
                var pluginVersions = pluginsByName[outdatedPlugin.Key.Name]
                    .Select(plugin => plugin.Version);

                table.AddRow(
                    new Text(outdatedPlugin.Key.Name),
                    new Markup($"[yellow]{string.Join(", ", pluginVersions)}[/]"),
                    new Markup($"[green]{outdatedPlugin.Value}[/]"));
            }

            SpectreConsole.Render(table, newLineBefore: true, newLineAfter: true);
        }
    }
}