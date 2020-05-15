using System.Text.Json;
using DotnetSpawn.Configuration;
using DotnetSpawn.Extensions;
using DotnetSpawn.IO;
using DotnetSpawn.Plugins;
using Json.More;
using Json.Schema;

namespace DotnetSpawn.Templating.Schema
{
    internal class TemplateSchemaGenerator
    {
        private readonly string _templateSchemaPath;
        private readonly InputJsonSchemaGenerator _inputSchemaGenerator;
        private readonly SpectreConsole _console;

        public TemplateSchemaGenerator(
            DotnetSpawnConfiguration configuration,
            InputJsonSchemaGenerator inputSchemaGenerator,
            SpectreConsole console)
        {
            _templateSchemaPath = configuration.SpawnTemplateSchemaPath;
            _inputSchemaGenerator = inputSchemaGenerator;
            _console = console;
        }

        public void Generate(SpawnPointPluginCollection plugins)
        {
            _console.LogDebug($"Generating spawn template schema of plugins: {string.Join(", ", plugins)}");

            var spawnPointMetadatas = plugins.GetSpawnPoints()
                .Select(spawnPoint => new SpawnPointMetadata(spawnPoint))
                .ToList();

            var validQualifiersPerSpawnPoint = spawnPointMetadatas
                .GetValidQualifiersPerSpawnPoint();

            var schema = new JsonSchemaBuilder()
                .Schema("http://json-schema.org/draft-07/schema#")
                .Type(SchemaValueType.Object)
                .Properties(
                    (
                        "name",
                        new JsonSchemaBuilder()
                            .Type(SchemaValueType.String)
                            .Description("Name of the spawn template")
                    ),
                    (
                        "description",
                        new JsonSchemaBuilder()
                            .Type(SchemaValueType.String)
                            .Description("Description of the spawn template")
                    ),
                    (
                        "steps",
                        new JsonSchemaBuilder()
                            .Type(SchemaValueType.Array)
                            .Description("The list of steps to perform for this template")
                            .Items(new JsonSchemaBuilder()
                                .Type(SchemaValueType.Object)
                                .Properties(
                                    (
                                        "name",
                                        new JsonSchemaBuilder()
                                            .Type(SchemaValueType.String | SchemaValueType.Null)
                                            .Description(
                                                "An optional name of the step. Makes it easier to reference " +
                                                "outputs of this step in subsequent steps.")
                                    ),
                                    (
                                        "description",
                                        new JsonSchemaBuilder()
                                            .Type(SchemaValueType.String | SchemaValueType.Null)
                                            .Description("An optional description of the spawn step")
                                    ),
                                    (
                                        "spawnPointId",
                                        new JsonSchemaBuilder()
                                            .Type(SchemaValueType.String)
                                            .Description("The ID of the spawn point to invoke in this step")
                                            .Enum(validQualifiersPerSpawnPoint
                                                .SelectMany(group => group)
                                                .Select(qualifier => (JsonElementProxy)qualifier))
                                    ))
                                .Required("spawnPointId")
                                .AllOf(spawnPointMetadatas
                                    .Where(spawnPoint => spawnPoint.HasInputs)
                                    .Select(spawnPoint =>
                                    {
                                        _console.LogDebug($"Generating input schema of {spawnPoint}");

                                        var spawnPointQualifiers = validQualifiersPerSpawnPoint[spawnPoint.Fqn];

                                        return new JsonSchemaBuilder()
                                            .If(new JsonSchemaBuilder()
                                                .Properties(("spawnPointId", new JsonSchemaBuilder()
                                                    .Enum(spawnPointQualifiers.Select(qualifier => (JsonElementProxy)qualifier)))))
                                            .Then(new JsonSchemaBuilder()
                                                .Properties(
                                                    (
                                                        "inputs",
                                                        _inputSchemaGenerator.GenerateSchemaOf(spawnPoint.Inputs)
                                                    ))
                                                .Required("spawnPointId", "inputs"))
                                                .Build();
                                    })))
                    ))
                .Required("name", "steps")
                .Build();

            _console.LogDebug($"Writing template schema to {_templateSchemaPath}");

            File.WriteAllBytes(_templateSchemaPath, JsonSerializer.SerializeToUtf8Bytes(schema));
        }
    }
}