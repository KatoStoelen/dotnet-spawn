using System.Text.RegularExpressions;

namespace DotnetSpawn.Templating
{
    internal class SpawnTemplate
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public IReadOnlyList<SpawnStep> Steps { get; set; }

        public static string GetFileName(string templateName)
        {
            return Regex.Replace(templateName.ToLowerInvariant(), @"\W", "-") + ".json";
        }
    }
}