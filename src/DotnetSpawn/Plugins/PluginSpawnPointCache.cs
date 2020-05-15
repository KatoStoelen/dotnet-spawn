using System.Text.Json;
using DotnetSpawn.Configuration;

namespace DotnetSpawn.Plugins
{
    internal class PluginSpawnPointCache
    {
        private readonly Dictionary<string, Entry> _cache;

        private PluginSpawnPointCache()
        {
            _cache = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
        }

        private PluginSpawnPointCache(Dictionary<string, Entry> cache)
        {
            _cache = new Dictionary<string, Entry>(
                cache, StringComparer.OrdinalIgnoreCase);
        }

        public void Add(SpawnPointPlugin plugin)
        {
            var spawnPoints =
                from spawnPoint in plugin.GetSpawnPoints()
                select new Entry.SpawnPoint
                {
                    TypeName = spawnPoint.Type.FullName,
                    Aliases = new SpawnPointMetadata(spawnPoint).Fqn.Aliases.ToList()
                };

            _cache.Add(plugin.PluginDirectory.Name, new Entry { SpawnPoints = spawnPoints.ToList() });
        }

        public void Remove(string pluginDirectory)
        {
            if (Path.IsPathRooted(pluginDirectory))
            {
                pluginDirectory = new DirectoryInfo(pluginDirectory).Name;
            }

            _ = _cache.Remove(pluginDirectory);
        }

        public void Clear()
        {
            _cache.Clear();
        }

        public (string PluginDirectory, string SpawnPointType) LookupSpawnPoint(string spawnPointId)
        {
            var matches = _cache
                .Select(pair =>
                    (
                        Directory: pair.Key,
                        SpawnPoint: pair.Value.SpawnPoints
                            .SingleOrDefault(point =>
                                point.Aliases.Any(alias =>
                                    alias.Equals(spawnPointId, StringComparison.OrdinalIgnoreCase)))
                    )
                )
                .Where(pair => pair.SpawnPoint != null)
                .ToList();

            if (matches.Count > 1)
            {
                throw new AmbiguousSpawnPointIdException(
                    spawnPointId, matches.Select(pair => pair.Directory));
            }

            if (!matches.Any())
            {
                throw new SpawnPointNotFoundException(spawnPointId);
            }

            var match = matches.Single();

            return (match.Directory, match.SpawnPoint.TypeName);
        }

        public void Save()
        {
            var cachePath = DotnetSpawnConfiguration.PluginCacheFile;
            using var fileStream = new FileStream(cachePath, FileMode.Create, FileAccess.Write);

            JsonSerializer.Serialize(
                fileStream, _cache, DotnetSpawnConfiguration.JsonSerializerOptions);
        }

        public static PluginSpawnPointCache Load()
        {
            var cachePath = DotnetSpawnConfiguration.PluginCacheFile;

            if (!File.Exists(cachePath))
            {
                return new PluginSpawnPointCache();
            }

            using var fileStream = File.OpenRead(cachePath);
            var cache = JsonSerializer.Deserialize<Dictionary<string, Entry>>(
                fileStream,
                DotnetSpawnConfiguration.JsonSerializerOptions);

            return new PluginSpawnPointCache(cache);
        }

        public class Entry
        {
            public ICollection<SpawnPoint> SpawnPoints { get; set; }

            public class SpawnPoint
            {
                public string TypeName { get; set; }
                public ICollection<string> Aliases { get; set; }
            }
        }
    }

    internal class AmbiguousSpawnPointIdException : Exception
    {
        public AmbiguousSpawnPointIdException(string spawnPointId, IEnumerable<string> plugins)
            : base($"Ambiguous spawn point ID: {spawnPointId}. Found in {string.Join(", ", plugins)}")
        {
        }
    }

    internal class SpawnPointNotFoundException : Exception
    {
        public SpawnPointNotFoundException(string spawnPointId)
            : base($"Spawn point '{spawnPointId}' not found")
        {
        }
    }
}