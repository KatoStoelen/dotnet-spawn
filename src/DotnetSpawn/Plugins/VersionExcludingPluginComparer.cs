namespace DotnetSpawn.Plugins
{
    internal class VersionExcludingPluginComparer : IEqualityComparer<SpawnPointPlugin>
    {
        private readonly StringComparer _ordinalIgnoreCase = StringComparer.OrdinalIgnoreCase;

        public bool Equals(SpawnPointPlugin plugin, SpawnPointPlugin other)
        {
            if (plugin == null || other == null)
            {
                return false;
            }

            if (ReferenceEquals(plugin, other))
            {
                return true;
            }

            return _ordinalIgnoreCase.Equals(plugin.Name, other?.Name);
        }

        public int GetHashCode(SpawnPointPlugin plugin)
        {
            return _ordinalIgnoreCase.GetHashCode(plugin.Name);
        }
    }
}