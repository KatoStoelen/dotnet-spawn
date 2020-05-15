using System.Text.Json;
using DotnetSpawn.Configuration;
using DotnetSpawn.Extensions;
using DotnetSpawn.IO;
using DotnetSpawn.Plugins;
using DotnetSpawn.Templating.Deserialization;

namespace DotnetSpawn.Templating
{
    internal class TemplateLoader
    {
        private readonly string _templateRootDirectory;
        private readonly SpawnPointPluginLoader _pluginLoader;
        private readonly SpectreConsole _console;
        private readonly JsonSerializerOptions _serializerOptions;

        public TemplateLoader(
            DotnetSpawnConfiguration configuration,
            SpawnPointPluginLoader pluginLoader,
            SpectreConsole console)
        {
            _templateRootDirectory = configuration.TemplateRootDirectory;
            _pluginLoader = pluginLoader;
            _console = console;
            _serializerOptions = new JsonSerializerOptions(
                DotnetSpawnConfiguration.JsonSerializerOptions)
            {
                Converters =
                {
                    new SpawnStepTypeConverter(_pluginLoader)
                }
            };
        }

        public IEnumerable<SpawnTemplate> LoadAll()
        {
            if (!Directory.Exists(_templateRootDirectory))
            {
                yield break;
            }

            var jsonFiles = Directory.EnumerateFiles(
                _templateRootDirectory, "*.json", SearchOption.AllDirectories);

            foreach (var jsonFile in jsonFiles)
            {
                SpawnTemplate template;

                try
                {
                    template = LoadInternal(new FileInfo(jsonFile));
                }
                catch (Exception e)
                {
                    _console.LogWarning($"Failed to load {jsonFile}: {e.Message}");

                    continue;
                }

                yield return template;
            }
        }

        public SpawnTemplate Load(string templateName)
        {
            var matches = LoadAll()
                .Where(template => template.Name.Equals(
                    templateName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!matches.Any())
            {
                throw new ArgumentException(
                    $"Template '{templateName}' not found in {_templateRootDirectory}");
            }

            if (matches.Count > 1)
            {
                throw new ArgumentException(
                    $"More than one template with name '{templateName}' found");
            }

            return matches.Single();
        }

        public SpawnTemplate Load(FileInfo templateFile)
        {
            if (!templateFile.Exists)
            {
                throw new FileNotFoundException($"Template file not found: {templateFile.FullName}");
            }

            return LoadInternal(templateFile);
        }

        private SpawnTemplate LoadInternal(FileInfo templateFile)
        {
            var fileName = templateFile.FullName.IsSubPathOf(_templateRootDirectory)
                ? templateFile.Name
                : templateFile.FullName;

            _console.LogDebug($"Loading template: {fileName}");

            using var fileStream = templateFile.OpenRead();
            var template = JsonSerializer.Deserialize<SpawnTemplate>(
                fileStream, _serializerOptions);

            _console.LogDebug($"Successfully loaded template: {fileName}");

            return template;
        }
    }
}