using System.ComponentModel;
using DotnetSpawn.CLI.Commands.Plugins;
using DotnetSpawn.IO;
using DotnetSpawn.Plugins;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using Spectre.Console.Cli;

namespace DotnetSpawn.Cli.Commands.Plugins
{
    [Description("Install a plugin")]
    internal class PluginInstallCommand : CancellableAsyncCommand<PluginInstallCommand.Settings>
    {
        public class Settings : NuGetCommonSettings
        {
            [CommandArgument(0, "<PLUGIN>")]
            [Description("The name of the plugin to install (a.k.a the NuGet package ID)")]
            public string PluginId { get; set; }

            [CommandOption("-v|--version <VERSION>")]
            [Description("The version of the plugin to install. If not specified, the latest version will be used")]
            public string Version { get; set; }

            [CommandOption("-f|--force")]
            [Description("Force a re-install of the plugin")]
            public bool Force { get; set; }

            public PackageIdentity GetPackageIdentity()
            {
                return new PackageIdentity(
                    PluginId,
                    !string.IsNullOrWhiteSpace(Version)
                        ? new NuGetVersion(Version)
                        : null);
            }
        }

        private readonly PluginInstaller _pluginInstaller;
        private readonly SpectreConsole _console;

        public PluginInstallCommand(PluginInstaller pluginInstaller, SpectreConsole console)
        {
            _pluginInstaller = pluginInstaller;
            _console = console;
        }

        protected override async Task<int> ExecuteAsync(Settings settings, CancellationToken cancellationToken)
        {
            try
            {
                var package = settings.GetPackageIdentity();

                var wasInstalled = await _pluginInstaller.InstallAsync(
                    package,
                    new PluginInstaller.Settings
                    {
                        PackageSource = settings.GetPackageSource(),
                        NuGetConfigFile = settings.NuGetConfigFile,
                        IncludePrerelease = settings.IncludePrerelease,
                        Force = settings.Force
                    },
                    cancellationToken);

                return wasInstalled ? 0 : 1;
            }
            catch (Exception e)
            {
                _console.LogError(e.Message, e);

                return 1;
            }
        }
    }
}