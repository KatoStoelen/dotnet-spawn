using DotnetSpawn.IO;
using DotnetSpawn.NuGet;
using DotnetSpawn.Templating.Schema;
using NuGet.Configuration;
using NuGet.Packaging.Core;

namespace DotnetSpawn.Plugins
{
    internal class PluginUpgrader : PluginOperationWithSideEffects
    {
        private readonly NuGetPackageFinder _packageFinder;
        private readonly NuGetPackageRemover _packageRemover;
        private readonly NuGetPackageInstaller _packageInstaller;
        private readonly SpectreConsole _console;

        public PluginUpgrader(
                NuGetPackageFinder packageFinder,
                NuGetPackageRemover packageRemover,
                NuGetPackageInstaller packageInstaller,
                SpawnPointPluginLoader pluginLoader,
                TemplateSchemaGenerator schemaGenerator,
                PluginSpawnPointCache spawnPointCache,
                SpectreConsole console)
            : base(pluginLoader, schemaGenerator, spawnPointCache, console)
        {
            _packageFinder = packageFinder;
            _packageRemover = packageRemover;
            _packageInstaller = packageInstaller;
            _console = console;
        }

        public async Task<bool> Upgrade(
            IReadOnlyCollection<SpawnPointPlugin> pluginVersionsToUpgrade,
            Settings settings,
            CancellationToken cancellationToken)
        {
            var packageIdentity = new PackageIdentity(
                pluginVersionsToUpgrade.First().Name, version: null);

            _console.LogInformation($"Looking up latest version of {packageIdentity}");

            var latestVersionResult = await _packageFinder.FindAsync(
                packageIdentity,
                settings.PackageSource,
                settings.NuGetConfigFile,
                settings.IncludePrerelease,
                cancellationToken);

            if (!latestVersionResult.Found)
            {
                _console.LogError($"Package {packageIdentity} not found");

                return false;
            }

            _console.LogInformation($"Latest version found is {latestVersionResult.Package.Version}");

            var latestVersionAlreadyInstalled = pluginVersionsToUpgrade
                .Any(plugin => plugin.Version >= latestVersionResult.Package.Version);

            if (latestVersionAlreadyInstalled)
            {
                _console.LogWarning(
                    $"Latest version of {packageIdentity} already installed ({latestVersionResult.Package.Version})");

                return false;
            }

            _console.LogWarning("Currently installed plugin version(s) will be uninstalled");

            if (!settings.SkipConfirmation && !_console.Confirm($"Upgrade plugin {packageIdentity}?"))
            {
                throw new OperationCanceledException();
            }

            _console.LogInformation($"Installing {latestVersionResult.Package}");

            var installResult = await _packageInstaller.InstallAsync(
                latestVersionResult, force: false, cancellationToken);

            var newPluginDirectoryPath = ((NuGetPackageInstallResult.Installed)installResult)
                .PluginDirectoryPath;

            _console.LogInformation($"Removing previously installed versions of {packageIdentity}");

            var uninstalledPluginPaths = new List<string>();

            foreach (var oldPluginVersion in pluginVersionsToUpgrade)
            {
                uninstalledPluginPaths.Add(
                    _packageRemover.RemovePackage(oldPluginVersion.PackageIdentity));
            }

            ApplyUpgradePluginSideEffects(uninstalledPluginPaths, newPluginDirectoryPath);

            _console.LogInformation($"{packageIdentity} successfully upgraded");

            return true;
        }

        public class Settings
        {
            public PackageSource PackageSource { get; set; }
            public FileInfo NuGetConfigFile { get; set; }
            public bool IncludePrerelease { get; set; }
            public bool SkipConfirmation { get; set; }
        }
    }
}
