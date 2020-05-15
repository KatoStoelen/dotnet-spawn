using System.ComponentModel;
using DotnetSpawn.Cli.Commands;
using DotnetSpawn.Cli.TypeConverters;
using NuGet.Configuration;
using Spectre.Console.Cli;

namespace DotnetSpawn.CLI.Commands.Plugins
{
    internal class NuGetCommonSettings : GlobalSettings
    {
        [CommandOption("-c|--nuget-config <NUGET_CONFIG>")]
        [Description("The NuGet configuration file to use")]
        [TypeConverter(typeof(FileInfoTypeConverter))]
        public FileInfo NuGetConfigFile { get; set; }

        [CommandOption("-s|--source <SOURCE>")]
        [Description("The NuGet source to use")]
        public string PackageSource { get; set; }

        [CommandOption("-u|--username <USERNAME>")]
        [Description("Username of <SOURCE> if authenticated feed. Only applicable when --source is specified")]
        public string PackageSourceUsername { get; set; }

        [CommandOption("-p|--password <PASSWORD>")]
        [Description("Username of <SOURCE> if authenticated feed. Only applicable when --source is specified")]
        public string PackageSourcePassword { get; set; }

        [CommandOption("--prerelease")]
        [Description("Include pre-releases when determining latest version of a plugin. Only has an effect if --version is not specified")]
        public bool IncludePrerelease { get; set; }

        public PackageSource GetPackageSource()
        {
            if (string.IsNullOrWhiteSpace(PackageSource))
            {
                return null;
            }

            return new PackageSource(PackageSource)
            {
                Credentials = !string.IsNullOrWhiteSpace(PackageSourceUsername)
                    ? PackageSourceCredential.FromUserInput(
                        PackageSource,
                        PackageSourceUsername,
                        PackageSourcePassword,
                        storePasswordInClearText: true,
                        validAuthenticationTypesText: null)
                    : null
            };
        }
    }
}