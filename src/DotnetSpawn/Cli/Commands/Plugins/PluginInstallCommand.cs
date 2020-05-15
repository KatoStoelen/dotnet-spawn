using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpawn.Cli.TypeConverters;
using DotnetSpawn.IO;
using DotnetSpawn.NuGet;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using Spectre.Console.Cli;

namespace DotnetSpawn.Cli.Commands.Plugins
{
    [Description("Install a spawn point plugin")]
    internal class PluginInstallCommand : CancellableAsyncCommand<PluginInstallCommand.Settings>
    {
        public class Settings : GlobalSettings
        {
            [CommandArgument(0, "<PACKAGE_ID>")]
            [Description("The NuGet package ID of the plugin to install")]
            public string PackageId { get; set; }

            [CommandOption("-v|--version <VERSION>")]
            [Description("The version of the plugin to install. If not specified, the latest version will be installed")]
            public string Version { get; set; }

            [CommandOption("--config-file <NUGET_CONFIG>")]
            [Description("The NuGet configuration file to use when installing the plugin")]
            [TypeConverter(typeof(FileInfoTypeConverter))]
            public FileInfo NuGetConfigFilePath { get; set; }

            [CommandOption("--source <SOURCE>")]
            [Description("The NuGet source from which the plugin should be installed")]
            public string PackageSource { get; set; }

            [CommandOption("--username <USERNAME>")]
            [Description("Username of <SOURCE> if authenticated feed. Only applicable when --source is specified")]
            public string PackageSourceUsername { get; set; }

            [CommandOption("--password <PASSWORD>")]
            [Description("Username of <SOURCE> if authenticated feed. Only applicable when --source is specified")]
            public string PackageSourcePassword { get; set; }

            [CommandOption("--prerelease")]
            [Description("Include pre-releases when determining latest version of a plugin. Only has an effect if --version is not specified")]
            public bool IncludePrerelease { get; set; }

            [CommandOption("--force")]
            [Description("Force a re-install of the plugin even though it is already installed")]
            public bool Force { get; set; }
        }

        private readonly NuGetPackageInstaller _nugetInstaller;
        private readonly SpectreConsole _console;

        public PluginInstallCommand(NuGetPackageInstaller nugetInstaller, SpectreConsole console)
        {
            _nugetInstaller = nugetInstaller;
            _console = console;
        }

        protected override async Task<int> ExecuteAsync(Settings settings, CancellationToken cancellationToken)
        {
            try
            {
                var package = GetPackageIdentity(settings);
                var source = GetPackageSource(settings);

                _console.LogInformation($"Installing plugin {package}");

                NuGetPackageInstallResult result;

                if (source != null)
                {
                    result = await _nugetInstaller.InstallAsync(
                        package,
                        source,
                        settings.IncludePrerelease,
                        settings.Force,
                        cancellationToken);
                }
                else if (settings.NuGetConfigFilePath != null)
                {
                    result = await _nugetInstaller.InstallAsync(
                        package,
                        settings.NuGetConfigFilePath,
                        settings.IncludePrerelease,
                        settings.Force,
                        cancellationToken);
                }
                else
                {
                    result = await _nugetInstaller.InstallAsync(
                        package,
                        settings.IncludePrerelease,
                        settings.Force,
                        cancellationToken);
                }

                if (result == NuGetPackageInstallResult.Installed)
                {
                    _console.LogInformation($"Successfully installed plugin {package}");

                    return 0;
                }
                else if (result == NuGetPackageInstallResult.NotFound)
                {
                    _console.LogError($"Plugin {package} not found");
                }
                else if (result == NuGetPackageInstallResult.AlreadyInstalled)
                {
                    _console.LogWarning($"Plugin {package} already installed. Use --force to reinstall.");
                }

                return 1;
            }
            catch (Exception e)
            {
                _console.LogError(e.Message, e);

                return 1;
            }
        }

        private static PackageIdentity GetPackageIdentity(Settings settings)
        {
            return new PackageIdentity(
                settings.PackageId,
                !string.IsNullOrWhiteSpace(settings.Version)
                    ? new NuGetVersion(settings.Version)
                    : null);
        }

        public static PackageSource GetPackageSource(Settings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.PackageSource))
            {
                return null;
            }

            return new PackageSource(settings.PackageSource)
            {
                Credentials = !string.IsNullOrWhiteSpace(settings.PackageSourceUsername)
                    ? PackageSourceCredential.FromUserInput(
                        settings.PackageSource,
                        settings.PackageSourceUsername,
                        settings.PackageSourcePassword,
                        storePasswordInClearText: true,
                        validAuthenticationTypesText: null)
                    : null
            };
        }
    }
}