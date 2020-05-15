using DotnetSpawn.Plugins;
using Json.More;
using Json.Schema;

namespace DotnetSpawn.Templating.Schema
{
    internal class EnumInputSchemaGenerator : ISpecificInputJsonSchemaGenerator
    {
        public int Precedence => 30;

        public bool CanGenerateSchemaOf(SpawnPointMetadata.InputMetadata input)
        {
            return input.Type.IsEnum;
        }

        public JsonSchema GenerateSchemaOf(
            SpawnPointMetadata.InputMetadata input,
            InputJsonSchemaGenerator generator)
        {
            return new JsonSchemaBuilder()
                .Type(SchemaValueType.String)
                .Description(input.Description)
                .Enum(Enum.GetValues(input.Type).Cast<JsonElementProxy>());
        }
    }
}
