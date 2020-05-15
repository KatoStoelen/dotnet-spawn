using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DotnetSpawn.Plugins
{
    internal class SpawnPointPluginCollection : IDisposable, IReadOnlyCollection<SpawnPointPlugin>
    {
        private readonly List<SpawnPointPlugin> _plugins;

        public SpawnPointPluginCollection(IEnumerable<SpawnPointPlugin> plugins)
        {
            _plugins = new List<SpawnPointPlugin>(plugins);
        }

        public int Count => _plugins.Count;

        public IEnumerable<SpawnPointType> GetSpawnPoints()
        {
            return _plugins
                .Where(plugin => plugin.IsCompatible)
                .SelectMany(plugin => plugin.GetSpawnPoints());
        }

        public IEnumerator<SpawnPointPlugin> GetEnumerator() => _plugins.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Dispose()
        {
            foreach (var plugin in _plugins)
            {
                plugin.Dispose();
            }

            _plugins.Clear();
        }
    }
}
