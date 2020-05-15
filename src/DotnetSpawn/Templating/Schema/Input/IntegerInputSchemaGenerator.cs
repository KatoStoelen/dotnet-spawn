using DotnetSpawn.Extensions;
using DotnetSpawn.Plugins;
using Json.Schema;

namespace DotnetSpawn.Templating.Schema
{
    internal class IntegerInputSchemaGenerator : ISpecificInputJsonSchemaGenerator
    {
        public int Precedence => 70;

        public bool CanGenerateSchemaOf(SpawnPointMetadata.InputMetadata input)
        {
            return input.Type.IsIntegerType();
        }

        public JsonSchema GenerateSchemaOf(
            SpawnPointMetadata.InputMetadata input,
            InputJsonSchemaGenerator generator)
        {
            var schemaType = SchemaValueType.Integer | SchemaValueType.String;

            if (input.Type.IsNullable())
            {
                schemaType = SchemaValueType.Null;
            }

            return new JsonSchemaBuilder()
                .Type(schemaType)
                .Description(input.Description);
        }
    }
}
