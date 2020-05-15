using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace DotnetSpawn.Configuration
{
    internal class DotnetSpawnConfiguration
    {
        private static readonly DirectoryInfo s_configDirectory =
            new(Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData,
                    Environment.SpecialFolderOption.DoNotVerify),
                "dotnet-spawn"));

        private static readonly FileInfo s_configFile =
            new(Path.Combine(s_configDirectory.FullName, "config.json"));

        public string PluginRootDirectory { get; set; } =
            Path.Combine(s_configDirectory.FullName, ".plugins");

        public void Save()
        {
            if (!s_configDirectory.Exists)
            {
                _ = Directory.CreateDirectory(s_configDirectory.FullName);
            }

            File.WriteAllText(
                s_configFile.FullName,
                JsonSerializer.Serialize(this),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        }

        public static DotnetSpawnConfiguration Load()
        {
            if (!s_configFile.Exists)
            {
                var config = new DotnetSpawnConfiguration();

                config.Save();

                return config;
            }

            return JsonSerializer.Deserialize<DotnetSpawnConfiguration>(
                File.ReadAllText(s_configFile.FullName));
        }
    }
}