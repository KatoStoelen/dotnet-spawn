using System.Text.Json;
using System.Text.Json.Serialization;
using DotnetSpawn.Configuration;
using DotnetSpawn.Plugins;

namespace DotnetSpawn.Templating.Deserialization
{
    internal class SpawnStepTypeConverter : JsonConverter<SpawnStep>
    {
        private readonly SpawnPointPluginLoader _pluginLoader;

        public SpawnStepTypeConverter(SpawnPointPluginLoader pluginLoader)
        {
            _pluginLoader = pluginLoader;
        }

        public override SpawnStep Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected an object");
            }

            var spawnStep = new SpawnStep();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException("Expected a property");
                }

                var propertyName = reader.GetString();
                _ = reader.Read();

                switch (propertyName)
                {
                    case "name":
                        spawnStep.Name = reader.GetString();
                        break;
                    case "description":
                        spawnStep.Description = reader.GetString();
                        break;
                    case "spawnPointId":
                        spawnStep.SpawnPointId = reader.GetString();
                        break;
                    case "inputs":
                        if (string.IsNullOrEmpty(spawnStep.SpawnPointId))
                        {
                            throw new JsonException("'spawnPointId' must be set before 'inputs'");
                        }

                        spawnStep.Inputs = ReadInputs(ref reader, spawnStep.SpawnPointId);
                        break;
                    default:
                        throw new JsonException($"Invalid property name: {propertyName}");
                }
            }

            return spawnStep;
        }

        private ISpawnPointInputs ReadInputs(ref Utf8JsonReader reader, string spawnPointId)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var spawnPointMetadata = new SpawnPointMetadata(_pluginLoader.LoadSpawnPoint(spawnPointId));

            if (spawnPointMetadata == null)
            {
                throw new JsonException(
                    $"Cannot deserialize inputs of unknown spawn point '{spawnPointId}'");
            }

            if (!spawnPointMetadata.HasInputs)
            {
                throw new JsonException($"Spawn point '{spawnPointId}' has no inputs");
            }

            var inputsJsonElement = JsonSerializer.Deserialize<JsonElement>(
                ref reader, DotnetSpawnConfiguration.JsonSerializerOptions);

            return new SpawnPointInputsProxy(inputsJsonElement, spawnPointMetadata.Inputs);
        }

        public override void Write(
            Utf8JsonWriter writer,
            SpawnStep value,
            JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }
    }
}