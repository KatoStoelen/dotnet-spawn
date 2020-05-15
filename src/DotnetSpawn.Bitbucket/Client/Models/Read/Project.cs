using System.Collections.Generic;

namespace DotnetSpawn.Points.Bitbucket.Client.Models.Read
{
    internal class Project
    {
        public long Id { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Public { get; set; }
        public string Type { get; set; }
        public Dictionary<string, List<Link>> Links { get; set; }
    }
}