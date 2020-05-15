using DotnetSpawn.Extensions;
using DotnetSpawn.Plugins;
using Json.Schema;

namespace DotnetSpawn.Templating.Schema
{
    internal class FloatingPointInputSchemaGenerator : ISpecificInputJsonSchemaGenerator
    {
        public int Precedence => 80;

        public bool CanGenerateSchemaOf(SpawnPointMetadata.InputMetadata input)
        {
            return input.Type.IsFloatingPointType();
        }

        public JsonSchema GenerateSchemaOf(
            SpawnPointMetadata.InputMetadata input,
            InputJsonSchemaGenerator generator)
        {
            var schemaType = SchemaValueType.Number | SchemaValueType.String;

            if (input.Type.IsNullable())
            {
                schemaType |= SchemaValueType.Null;
            }

            return new JsonSchemaBuilder()
                .Type(schemaType)
                .Description(input.Description);
        }
    }
}
