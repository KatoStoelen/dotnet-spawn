using DotnetSpawn.Plugins;
using Json.Schema;

namespace DotnetSpawn.Templating.Schema
{
    internal class StringInputSchemaGenerator : ISpecificInputJsonSchemaGenerator
    {
        public int Precedence => 20;

        public bool CanGenerateSchemaOf(SpawnPointMetadata.InputMetadata input)
        {
            return input.Type == typeof(string);
        }

        public JsonSchema GenerateSchemaOf(
            SpawnPointMetadata.InputMetadata input,
            InputJsonSchemaGenerator generator)
        {
            return new JsonSchemaBuilder()
                .Type(SchemaValueType.String)
                .Description(input.Description);
        }
    }
}
