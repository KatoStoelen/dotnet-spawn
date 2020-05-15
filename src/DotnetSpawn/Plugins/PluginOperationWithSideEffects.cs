using DotnetSpawn.IO;
using DotnetSpawn.Templating.Schema;

namespace DotnetSpawn.Plugins
{
    internal class PluginOperationWithSideEffects
    {
        private readonly SpawnPointPluginLoader _pluginLoader;
        private readonly TemplateSchemaGenerator _schemaGenerator;
        private readonly PluginSpawnPointCache _spawnPointCache;
        private readonly SpectreConsole _console;

        protected PluginOperationWithSideEffects(
            SpawnPointPluginLoader pluginLoader,
            TemplateSchemaGenerator schemaGenerator,
            PluginSpawnPointCache spawnPointCache,
            SpectreConsole console)
        {
            _pluginLoader = pluginLoader;
            _schemaGenerator = schemaGenerator;
            _spawnPointCache = spawnPointCache;
            _console = console;
        }

        protected void ApplyAddPluginSideEffects(string pluginDirectoryPath)
        {
            var plugins = _pluginLoader.LoadAllPlugins();

            RegenerateTemplateSchema(plugins);
            AddPluginToCache(pluginDirectoryPath, plugins);
        }

        protected void ApplyRemovePluginSideEffects(string pluginDirectoryPath)
        {
            var plugins = _pluginLoader.LoadAllPlugins();

            RegenerateTemplateSchema(plugins);
            RemovePluginsFromCache(new[] { pluginDirectoryPath });
        }

        protected void ApplyUpgradePluginSideEffects(
            IEnumerable<string> uninstalledPluginPaths, string newPluginDirectoryPath)
        {
            var plugins = _pluginLoader.LoadAllPlugins();

            RegenerateTemplateSchema(plugins);
            AddPluginToCache(newPluginDirectoryPath, plugins);
            RemovePluginsFromCache(uninstalledPluginPaths);
        }

        private void RegenerateTemplateSchema(SpawnPointPluginCollection plugins)
        {
            _console.LogDebug("Regenerating template schema");

            try
            {
                _schemaGenerator.Generate(plugins);
            }
            catch (Exception e)
            {
                _console.LogError($"Failed to regenerate spawn template schema: {e.Message}", e);
            }
        }

        private void AddPluginToCache(string pluginDirectoryPath, SpawnPointPluginCollection plugins)
        {
            _console.LogDebug($"Adding {pluginDirectoryPath} to cache");

            try
            {
                var newlyAddedPlugin = plugins
                    .Single(plugin => plugin.ResidesInDirectory(pluginDirectoryPath));

                _spawnPointCache.Add(newlyAddedPlugin);
                _spawnPointCache.Save();
            }
            catch (Exception e)
            {
                _console.LogError($"Failed to add plugin to cache: {e.Message}", e);
            }
        }

        private void RemovePluginsFromCache(IEnumerable<string> pluginDirectoryPaths)
        {
            try
            {
                foreach (var pluginDir in pluginDirectoryPaths)
                {
                    _console.LogDebug($"Removing {pluginDir} from cache");
                    _spawnPointCache.Remove(pluginDir);
                }

                _spawnPointCache.Save();
            }
            catch (Exception e)
            {
                _console.LogError($"Failed to remove plugin from cache: {e.Message}", e);
            }
        }
    }
}