using System;
using System.IO;
using System.Linq;
using DotnetSpawn.IO;
using NuGet.Frameworks;
using NuGet.Packaging;

namespace DotnetSpawn.Plugins
{
    internal class SpawnPointPluginLoader
    {
        private readonly string _rootDirectory;
        private readonly SpectreConsole _console;

        public SpawnPointPluginLoader(string rootDirectory, SpectreConsole console)
        {
            _rootDirectory = rootDirectory;
            _console = console;
        }

        public SpawnPointPluginCollection LoadPlugins()
        {
            var nugetFramework = FrameworkConstants.CommonFrameworks.Net50;

            var plugins = Directory
                .EnumerateDirectories(_rootDirectory)
                .Select(pluginDirectory =>
                {
                    _console.LogDebug($"Loading plugin in directory {pluginDirectory}");

                    using var packageReader = new PackageFolderReader(pluginDirectory);

                    var packageIdentity = packageReader.GetIdentity();
                    var nuspecReader = packageReader.NuspecReader;

                    _console.LogDebug($"Plugin name: {packageIdentity.Id}");
                    _console.LogDebug($"Plugin version: {packageIdentity.Version}");

                    var pluginFrameworks = packageReader.GetSupportedFrameworks().ToList().AsReadOnly();
                    var compatiblePluginFramework = new FrameworkReducer()
                        .GetNearest(nugetFramework, pluginFrameworks);

                    _console.LogDebug($"Plugin framework(s): {string.Join(", ", pluginFrameworks)}");

                    if (compatiblePluginFramework == null)
                    {
                        _console.LogDebug($"Plugin is not compatible with current framework ({nugetFramework})");

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
                        new Uri(Path.Combine(pluginDirectory, mainAssemblyItem)).LocalPath);

                    _console.LogDebug($"Loaded plugin assembly: {mainAssemblyPath}");

                    return new SpawnPointPlugin(
                        mainAssemblyPath,
                        packageIdentity,
                        nuspecReader,
                        pluginFrameworks,
                        compatiblePluginFramework);
                });

            return new SpawnPointPluginCollection(plugins);
        }
    }
}