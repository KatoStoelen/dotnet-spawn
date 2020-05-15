using System.Text;
using System.Text.Json;

namespace DotnetSpawn.Configuration
{
    internal class DotnetSpawnConfiguration
    {
        public static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        private static readonly DirectoryInfo s_configDirectory =
            new(Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData,
                    Environment.SpecialFolderOption.DoNotVerify),
                "dotnet-spawn"));

        private static readonly string s_configFile =
            Path.Combine(s_configDirectory.FullName, "config.json");

        public static string PluginCacheFile =>
            Path.Combine(s_configDirectory.FullName, "cache.json");

        public string PluginRootDirectory { get; set; } =
            Path.Combine(s_configDirectory.FullName, ".plugins");

        public string TemplateRootDirectory { get; set; } =
            Path.Combine(s_configDirectory.FullName, ".templates");

        public string SpawnTemplateSchemaPath { get; set; } =
            Path.Combine(s_configDirectory.FullName, "template.schema.json");

        public void Save()
        {
            if (!s_configDirectory.Exists)
            {
                s_configDirectory.Create();
            }

            File.WriteAllText(
                s_configFile,
                JsonSerializer.Serialize(this, JsonSerializerOptions),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        }

        public static DotnetSpawnConfiguration Load()
        {
            if (!File.Exists(s_configFile))
            {
                var config = new DotnetSpawnConfiguration();

                config.Save();

                return config;
            }

            using var fileStream = File.OpenRead(s_configFile);

            return JsonSerializer.Deserialize<DotnetSpawnConfiguration>(
                fileStream, JsonSerializerOptions);
        }
    }
}