using System.ComponentModel;
using DotnetSpawn.IO;
using DotnetSpawn.Plugin;
using DotnetSpawn.Plugins;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using Spectre.Console.Cli;

namespace DotnetSpawn.Cli.Commands.Plugins
{
    [Description("Uninstall a plugin")]
    internal class PluginUninstallCommand : Command<PluginUninstallCommand.Settings>
    {
        public class Settings : GlobalSettings
        {
            [CommandArgument(0, "[PLUGIN]")]
            [Description("Name of the plugin to uninstall. If not specified, a prompt will be shown")]
            public string PluginName { get; set; }

            [CommandOption("-v|--version <VERSION>")]
            [Description("Version of the plugin to uninstall. Only applicable if a plugin name is specified")]
            public string Version { get; set; }

            [CommandOption("-y|--yes")]
            [Description("Automatically approve the plugin uninstall")]
            public bool SkipConfirmation { get; set; }
        }

        private readonly SpawnPointPluginLoader _pluginLoader;
        private readonly PluginUninstaller _pluginUninstaller;
        private readonly SpectreConsole _console;

        public PluginUninstallCommand(
            SpawnPointPluginLoader pluginLoader,
            PluginUninstaller pluginUninstaller,
            SpectreConsole console)
        {
            _pluginLoader = pluginLoader;
            _pluginUninstaller = pluginUninstaller;
            _console = console;
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            try
            {
                var package = GetPackageIdentity(settings.PluginName, settings.Version);

                if (package == null)
                {
                    _console.LogError($"Unknown plugin: {settings.PluginName}");

                    return 1;
                }

                var wasUninstalled = _pluginUninstaller.Uninstall(package, settings.SkipConfirmation);

                return wasUninstalled ? 0 : 1;
            }
            catch (OperationCanceledException)
            {
                _console.LogDebug("Plugin uninstall cancelled");
                return -1;
            }
            catch (Exception e)
            {
                _console.LogError(e.Message, e);

                return 1;
            }
        }

        private PackageIdentity GetPackageIdentity(string pluginName, string version)
        {
            if (!string.IsNullOrWhiteSpace(pluginName) && !string.IsNullOrWhiteSpace(version))
            {
                return new PackageIdentity(pluginName, new NuGetVersion(version));
            }
            else
            {
                var plugins = _pluginLoader.LoadAllPlugins();

                if (string.IsNullOrWhiteSpace(pluginName))
                {
                    return PromptPluginToRemove(plugins);
                }
                else
                {
                    var filteredPlugins = plugins
                        .Where(plugin => plugin.Name.Equals(pluginName, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (!filteredPlugins.Any())
                    {
                        return null;
                    }
                    else
                    {
                        return filteredPlugins.Count == 1
                            ? filteredPlugins.Single().PackageIdentity
                            : PromptPluginToRemove(filteredPlugins);
                    }
                }
            }
        }

        private PackageIdentity PromptPluginToRemove(IEnumerable<SpawnPointPlugin> plugins)
        {
            var options = plugins
                .Select(plugin =>
                    new PromptOption<PackageIdentity>(
                        plugin.PackageIdentity, plugin.ToString()));

            return _console.Prompt(options, "Select plugin to remove:");
        }
    }
}