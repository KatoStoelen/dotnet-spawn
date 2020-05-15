using DotnetSpawn.IO;
using DotnetSpawn.NuGet;
using DotnetSpawn.Templating.Schema;
using NuGet.Configuration;
using NuGet.Packaging.Core;

namespace DotnetSpawn.Plugins
{
    internal class PluginInstaller : PluginOperationWithSideEffects
    {
        private readonly NuGetPackageInstaller _packageInstaller;
        private readonly SpectreConsole _console;

        public PluginInstaller(
                NuGetPackageInstaller packageInstaller,
                SpawnPointPluginLoader pluginLoader,
                TemplateSchemaGenerator schemaGenerator,
                PluginSpawnPointCache spawnPointCache,
                SpectreConsole console)
            : base(pluginLoader, schemaGenerator, spawnPointCache, console)
        {
            _packageInstaller = packageInstaller;
            _console = console;
        }

        public async Task<bool> InstallAsync(
            PackageIdentity pluginPackage,
            Settings settings,
            CancellationToken cancellationToken)
        {
            _console.LogInformation($"Installing plugin {pluginPackage}");

            var result = await _packageInstaller.InstallAsync(
                pluginPackage,
                settings.PackageSource,
                settings.NuGetConfigFile,
                settings.IncludePrerelease,
                settings.Force,
                cancellationToken);

            if (result is NuGetPackageInstallResult.Installed installed)
            {
                ApplyAddPluginSideEffects(installed.PluginDirectoryPath);

                _console.LogInformation($"Successfully installed plugin {pluginPackage}");

                return true;
            }
            else if (result is NuGetPackageInstallResult.NotFound)
            {
                _console.LogError($"Plugin {pluginPackage} not found");
            }
            else if (result is NuGetPackageInstallResult.AlreadyInstalled)
            {
                _console.LogWarning($"Plugin {pluginPackage} already installed. Use --force to reinstall.");
            }

            return false;
        }

        public class Settings
        {
            public PackageSource PackageSource { get; set; }
            public FileInfo NuGetConfigFile { get; set; }
            public bool IncludePrerelease { get; set; }
            public bool Force { get; set; }
        }
    }
}
