using System.Collections.Generic;

namespace DotnetSpawn.Points.Bitbucket.Client.Models.Read
{
    internal class Repository
    {
        public string Id { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }
        public string ScmId { get; set; }
        public string State { get; set; }
        public string StatusMessage { get; set; }
        public string Description { get; set; }
        public bool Forkable { get; set; }
        public bool Public { get; set; }
        public Project Project { get; set; }
        public Dictionary<string, List<Link>> Links { get; set; }
    }
}