using DotnetSpawn.IO;
using DotnetSpawn.NuGet;
using DotnetSpawn.Templating.Schema;
using NuGet.Packaging.Core;

namespace DotnetSpawn.Plugins
{
    internal class PluginUninstaller : PluginOperationWithSideEffects
    {
        private readonly NuGetPackageRemover _packageRemover;
        private readonly SpectreConsole _console;

        public PluginUninstaller(
                NuGetPackageRemover packageRemover,
                SpawnPointPluginLoader pluginLoader,
                TemplateSchemaGenerator schemaGenerator,
                PluginSpawnPointCache spawnPointCache,
                SpectreConsole console)
            : base(pluginLoader, schemaGenerator, spawnPointCache, console)
        {
            _packageRemover = packageRemover;
            _console = console;
        }

        public bool Uninstall(PackageIdentity pluginPackage, bool skipConfirmation)
        {
            try
            {
                if (!skipConfirmation && !_console.Confirm($"Uninstall plugin {pluginPackage}?"))
                {
                    throw new OperationCanceledException();
                }

                var oldPackageDirectoryPath = _packageRemover.RemovePackage(pluginPackage);

                ApplyRemovePluginSideEffects(oldPackageDirectoryPath);

                _console.LogInformation($"Plugin {pluginPackage} successfully uninstalled");

                return true;
            }
            catch (DirectoryNotFoundException)
            {
                _console.LogError($"Could not determine install directory of plugin '{pluginPackage}'");

                return false;
            }
        }
    }
}
