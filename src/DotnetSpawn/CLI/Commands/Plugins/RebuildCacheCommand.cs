using System.ComponentModel;
using DotnetSpawn.IO;
using DotnetSpawn.Plugins;
using Spectre.Console.Cli;

namespace DotnetSpawn.CLI.Commands.Plugins
{
    [Description("Rebuild the spawn point cache")]
    internal class RebuildCacheCommand : Command
    {
        private readonly SpawnPointPluginLoader _pluginLoader;
        private readonly PluginSpawnPointCache _spawnPointCache;
        private readonly SpectreConsole _console;

        public RebuildCacheCommand(
            SpawnPointPluginLoader pluginLoader,
            PluginSpawnPointCache spawnPointCache,
            SpectreConsole console)
        {
            _pluginLoader = pluginLoader;
            _spawnPointCache = spawnPointCache;
            _console = console;
        }

        public override int Execute(CommandContext context)
        {
            try
            {
                _console.LogDebug($"Loading plugins");

                var plugins = _pluginLoader.LoadAllPlugins();

                _console.LogDebug($"Clearing cache");

                _spawnPointCache.Clear();

                _console.LogDebug($"Adding all plugins to cache");

                foreach (var plugin in plugins)
                {
                    _spawnPointCache.Add(plugin);
                }

                _console.LogDebug($"Saving cache to disk");

                _spawnPointCache.Save();

                _console.LogInformation("Spawn point cache successfully rebuilt");

                return 0;
            }
            catch (Exception e)
            {
                _console.LogError($"Failed to refresh spawn point cache: {e.Message}", e);

                return 1;
            }
        }
    }
}