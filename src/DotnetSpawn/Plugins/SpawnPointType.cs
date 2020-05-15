namespace DotnetSpawn.Plugins
{
    internal class SpawnPointType
    {
        public SpawnPointType(SpawnPointPlugin plugin, Type type)
        {
            Plugin = plugin;
            Type = type;
        }

        public SpawnPointPlugin Plugin { get; set; }
        public Type Type { get; }
    }
}