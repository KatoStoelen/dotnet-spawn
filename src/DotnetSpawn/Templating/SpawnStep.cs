namespace DotnetSpawn.Templating
{
    internal class SpawnStep
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string SpawnPointId { get; set; }
        public ISpawnPointInputs Inputs { get; set; }
    }
}
