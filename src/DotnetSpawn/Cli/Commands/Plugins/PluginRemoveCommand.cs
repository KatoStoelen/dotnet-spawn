using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DotnetSpawn.IO;
using DotnetSpawn.NuGet;
using DotnetSpawn.Plugin;
using DotnetSpawn.Plugins;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using Spectre.Console.Cli;

namespace DotnetSpawn.Cli.Commands.Plugins
{
    [Description("Remove an installed plugin")]
    internal class PluginRemoveCommand : Command<PluginRemoveCommand.Settings>
    {
        public class Settings : GlobalSettings
        {
            [CommandArgument(0, "[PLUGIN]")]
            [Description("Name of the plugin to remove. If not specified, a prompt will be shown")]
            public string PluginName { get; set; }

            [CommandOption("-v|--version <VERSION>")]
            [Description("Version of the plugin to remove. Only applicable if a plugin name is specified")]
            public string Version { get; set; }

            [CommandOption("-y|--yes")]
            [Description("Automatically approve the plugin removal")]
            public bool SkipConfirmation { get; set; }
        }

        private readonly SpawnPointPluginLoader _pluginLoader;
        private readonly NuGetPackageRemover _packageRemover;
        private readonly SpectreConsole _console;

        public PluginRemoveCommand(
            SpawnPointPluginLoader pluginLoader,
            NuGetPackageRemover packageRemover,
            SpectreConsole console)
        {
            _pluginLoader = pluginLoader;
            _packageRemover = packageRemover;
            _console = console;
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            try
            {
                var package = GetPackageIdentity(settings.PluginName, settings.Version);

                if (package == null)
                {
                    _console.LogError($"The specified plugin '{settings.PluginName}' is not recognized.");

                    return 1;
                }

                if (!settings.SkipConfirmation && !_console.Confirm($"Remove plugin {package}?"))
                {
                    _console.LogDebug("Plugin removal cancelled by user");
                    return 1;
                }

                _packageRemover.RemovePackage(package);

                _console.LogInformation($"Plugin {package} successfully removed");

                return 0;
            }
            catch (DirectoryNotFoundException)
            {
                _console.LogError($"Could not determine install directory of specified plugin.");

                return 1;
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
                var plugins = _pluginLoader.LoadPlugins();

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