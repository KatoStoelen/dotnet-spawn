using System.Text.Json;
using DotnetSpawn.Configuration;
using DotnetSpawn.Plugins;
using DotnetSpawn.Templating.Deserialization.Expressions;
using DotnetSpawn.Templating.Execution;

namespace DotnetSpawn.Templating.Deserialization
{
    internal class SpawnPointInputsProxy : ISpawnPointInputs
    {
        private readonly JsonElement _inputsJsonElement;
        private readonly SpawnPointMetadata.InputMetadata _inputMetadata;

        public SpawnPointInputsProxy(
            JsonElement inputsJsonElement,
            SpawnPointMetadata.InputMetadata inputMetadata)
        {
            _inputsJsonElement = inputsJsonElement;
            _inputMetadata = inputMetadata;
        }

        public object GetInstance(IExecutionContext executionContext)
        {
            return DeserializeInputObject(
                _inputsJsonElement,
                _inputMetadata,
                executionContext);
        }

        private object DeserializeInputObject(
            JsonElement jsonElement,
            SpawnPointMetadata.InputMetadata inputMetadata,
            IExecutionContext executionContext)
        {
            if (jsonElement.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException("Expected inputs to be an object");
            }

            var instance = Activator.CreateInstance(inputMetadata.Type);

            foreach (var jsonProperty in jsonElement.EnumerateObject())
            {
                var propertyMetadata = inputMetadata.GetPropertyMetadata(jsonProperty.Name);
                var propertyInfo = inputMetadata.Type.GetProperty(propertyMetadata.Name);

                if (jsonProperty.Value.ValueKind == JsonValueKind.Object)
                {
                    var objectValue = DeserializeInputObject(
                        jsonProperty.Value,
                        propertyMetadata,
                        executionContext);

                    propertyInfo.SetValue(instance, objectValue);

                    continue;
                }

                if (jsonProperty.Value.ValueKind == JsonValueKind.String)
                {
                    var stringValue = jsonProperty.Value.GetString();

                    if (stringValue.StartsWith("$[", StringComparison.Ordinal) &&
                        stringValue.EndsWith("]", StringComparison.Ordinal))
                    {
                        var expression = stringValue[2..^1];
                        var tokenResult = Tokenizer.Tokenize(expression);
                        var parsedExpression = ExpressionParser.Parse(tokenResult);
                        var valueFactory = parsedExpression.Compile();

                        propertyInfo.SetValue(instance, valueFactory(executionContext));

                        continue;
                    }
                }

                var value = jsonProperty.Value.Deserialize(
                    propertyMetadata.Type, DotnetSpawnConfiguration.JsonSerializerOptions);

                propertyInfo.SetValue(instance, value);
            }

            return instance;
        }
    }
}
