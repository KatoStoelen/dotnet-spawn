using System.ComponentModel;
using DotnetSpawn.Cli.Commands;
using DotnetSpawn.IO;
using DotnetSpawn.Plugin;
using DotnetSpawn.Plugins;
using Spectre.Console.Cli;

namespace DotnetSpawn.CLI.Commands.Plugins
{
    [Description("Upgrade a plugin to the latest version")]
    internal class PluginUpgradeCommand : CancellableAsyncCommand<PluginUpgradeCommand.Settings>
    {
        public class Settings : NuGetCommonSettings
        {
            [CommandArgument(0, "[PLUGIN]")]
            [Description("Name of the plugin to upgrade. If not specified, a prompt will be shown")]
            public string PluginName { get; set; }

            [CommandOption("-y|--yes")]
            [Description("Automatically approve the plugin upgrade")]
            public bool SkipConfirmation { get; set; }
        }

        private readonly SpawnPointPluginLoader _pluginLoader;
        private readonly PluginUpgrader _pluginUpgrader;
        private readonly SpectreConsole _console;

        public PluginUpgradeCommand(
            SpawnPointPluginLoader pluginLoader,
            PluginUpgrader pluginUpgrader,
            SpectreConsole console)
        {
            _pluginLoader = pluginLoader;
            _pluginUpgrader = pluginUpgrader;
            _console = console;
        }

        protected override async Task<int> ExecuteAsync(
            Settings settings, CancellationToken cancellationToken)
        {
            try
            {
                var pluginVersionsToUpgrade = GetPluginsToUpgrade(settings.PluginName);
                if (!pluginVersionsToUpgrade.Any())
                {
                    _console.LogError($"Unknown plugin: {settings.PluginName}");

                    return 1;
                }

                var wasUpgraded = await _pluginUpgrader.Upgrade(
                    pluginVersionsToUpgrade,
                    new PluginUpgrader.Settings
                    {
                        PackageSource = settings.GetPackageSource(),
                        NuGetConfigFile = settings.NuGetConfigFile,
                        IncludePrerelease = settings.IncludePrerelease,
                        SkipConfirmation = settings.SkipConfirmation
                    },
                    cancellationToken);

                return wasUpgraded ? 0 : 1;
            }
            catch (OperationCanceledException)
            {
                _console.LogDebug("Plugin upgrade cancelled");
                return -1;
            }
            catch (Exception e)
            {
                _console.LogError(e.Message, e);

                return 1;
            }
        }

        private IReadOnlyCollection<SpawnPointPlugin> GetPluginsToUpgrade(string pluginName)
        {
            var plugins = _pluginLoader.LoadAllPlugins();

            var pluginVersionsToUpgrade = new List<SpawnPointPlugin>();

            if (!string.IsNullOrWhiteSpace(pluginName))
            {
                pluginVersionsToUpgrade.AddRange(plugins
                    .Where(plugin =>
                        plugin.Name.Equals(
                            pluginName, StringComparison.OrdinalIgnoreCase)));
            }
            else
            {
                pluginVersionsToUpgrade.AddRange(PromptPluginsToUpgrade(plugins));
            }

            return pluginVersionsToUpgrade.AsReadOnly();
        }

        private IEnumerable<SpawnPointPlugin> PromptPluginsToUpgrade(
            IEnumerable<SpawnPointPlugin> plugins)
        {
            var options = plugins
                .GroupBy(plugin => plugin.Name, StringComparer.OrdinalIgnoreCase)
                .Select(group =>
                    new PromptOption<IEnumerable<SpawnPointPlugin>>(
                        group, group.Key));

            return _console.Prompt(options, "Select plugin to upgrade:");
        }
    }
}