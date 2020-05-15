using DotnetSpawn.Plugins;
using Json.Schema;

namespace DotnetSpawn.Templating.Schema
{
    internal class ObjectInputSchemaGenerator : ISpecificInputJsonSchemaGenerator
    {
        public int Precedence => int.MaxValue;

        public bool CanGenerateSchemaOf(SpawnPointMetadata.InputMetadata input)
        {
            return true;
        }

        public JsonSchema GenerateSchemaOf(
            SpawnPointMetadata.InputMetadata input,
            InputJsonSchemaGenerator generator)
        {
            return new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(
                    input.NestedInputs
                        .ToDictionary(
                            input => input.Name,
                            input => generator.GenerateSchemaOf(input)))
                .AdditionalProperties(false)
                .Required(
                    input.NestedInputs
                        .Where(input => input.Required)
                        .Select(input => input.Name));
        }
    }
}
