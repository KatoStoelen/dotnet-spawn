using DotnetSpawn.Configuration;
using DotnetSpawn.IO;
using NuGet.Frameworks;
using NuGet.Packaging;

namespace DotnetSpawn.Plugins
{
    internal class SpawnPointPluginLoader : IDisposable
    {
        private static readonly NuGetFramework s_currentNugetFramework =
            FrameworkConstants.CommonFrameworks.Net60;

        private readonly string _rootDirectory;
        private readonly PluginSpawnPointCache _spawnPointCache;
        private readonly SpectreConsole _console;
        private readonly Dictionary<string, SpawnPointPlugin> _loadedPlugins;

        public SpawnPointPluginLoader(
            DotnetSpawnConfiguration configuration,
            PluginSpawnPointCache spawnPointCache,
            SpectreConsole console)
        {
            _rootDirectory = configuration.PluginRootDirectory;
            _spawnPointCache = spawnPointCache;
            _console = console;
            _loadedPlugins = new(StringComparer.OrdinalIgnoreCase);
        }

        public SpawnPointPluginCollection LoadAllPlugins()
        {
            var plugins = Directory
                .EnumerateDirectories(_rootDirectory)
                .Select(LoadPlugin);

            return new SpawnPointPluginCollection(plugins);
        }

        private SpawnPointPlugin LoadPlugin(string pluginDirectoryPath)
        {
            if (!Path.IsPathRooted(pluginDirectoryPath))
            {
                pluginDirectoryPath = Path.Combine(_rootDirectory, pluginDirectoryPath);
            }

            var pluginDirectoryName = new DirectoryInfo(pluginDirectoryPath).Name;

            if (_loadedPlugins.TryGetValue(pluginDirectoryName, out var alreadyLoadedPlugin))
            {
                return alreadyLoadedPlugin;
            }

            _console.LogDebug($"Loading plugin in directory {pluginDirectoryPath}");

            using var packageReader = new PackageFolderReader(pluginDirectoryPath);

            var packageIdentity = packageReader.GetIdentity();
            var nuspecReader = packageReader.NuspecReader;

            _console.LogDebug($"Plugin name: {packageIdentity.Id}");
            _console.LogDebug($"Plugin version: {packageIdentity.Version}");

            var pluginFrameworks = packageReader.GetSupportedFrameworks().ToList().AsReadOnly();
            var compatiblePluginFramework = new FrameworkReducer()
                .GetNearest(s_currentNugetFramework, pluginFrameworks);

            _console.LogDebug($"Plugin framework(s): {string.Join(", ", pluginFrameworks)}");

            if (compatiblePluginFramework == null)
            {
                _console.LogDebug($"Plugin is not compatible with current framework ({s_currentNugetFramework})");

                return SpawnPointPlugin.IncompatiblePlugin(
                    packageIdentity, nuspecReader, pluginFrameworks);
            }

            _console.LogDebug($"Chosen framework: {compatiblePluginFramework}");

            var compatibleFrameworkItemGroup = packageReader
                .GetLibItems()
                .Single(libItem => libItem.TargetFramework.Equals(compatiblePluginFramework));

            foreach (var itemPath in compatibleFrameworkItemGroup.Items)
            {
                _console.LogTrace($"Plugin item: {itemPath}");
            }

            var mainAssemblySearchDirectory = Path.Combine(
                PackagingConstants.Folders.Lib,
                compatiblePluginFramework.GetShortFolderName());

            var mainAssemblyCandidates = compatibleFrameworkItemGroup.Items
                .Where(itemPath =>
                    Path.GetDirectoryName(itemPath).Equals(
                        mainAssemblySearchDirectory, StringComparison.OrdinalIgnoreCase) &&
                    Path.GetExtension(itemPath).Equals(
                        ".dll", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var candidatePath in mainAssemblyCandidates)
            {
                _console.LogTrace($"Main assembly candidate: {Path.GetFileName(candidatePath)}");
            }

            var mainAssemblyItem = mainAssemblyCandidates
                .SingleOrDefault(candidatePath =>
                    Path.GetFileNameWithoutExtension(candidatePath).Equals(
                        packageIdentity.Id, StringComparison.OrdinalIgnoreCase));

            if (mainAssemblyItem == null)
            {
                _console.LogDebug("Could not determine main plugin assembly path");

                return SpawnPointPlugin.IncompatiblePlugin(
                    packageIdentity, nuspecReader, pluginFrameworks, compatiblePluginFramework);
            }

            var mainAssemblyPath = Path.GetFullPath(
                new Uri(Path.Combine(pluginDirectoryPath, mainAssemblyItem)).LocalPath);

            _console.LogDebug($"Loaded plugin assembly: {mainAssemblyPath}");

            var plugin = new SpawnPointPlugin(
                pluginDirectoryPath,
                mainAssemblyPath,
                packageIdentity,
                nuspecReader,
                pluginFrameworks,
                compatiblePluginFramework);

            _loadedPlugins.Add(pluginDirectoryName, plugin);

            return plugin;
        }

        public SpawnPointType LoadSpawnPoint(string spawnPointId)
        {
            var (directory, spawnPointTypeName) = _spawnPointCache.LookupSpawnPoint(spawnPointId);

            var plugin = LoadPlugin(directory);

            return plugin.GetSpawnPoint(spawnPointTypeName);
        }

        public void Dispose()
        {
            foreach (var plugin in _loadedPlugins.Values)
            {
                plugin.Dispose();
            }

            _loadedPlugins.Clear();
        }
    }
}