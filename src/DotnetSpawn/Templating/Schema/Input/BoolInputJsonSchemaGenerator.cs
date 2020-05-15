using DotnetSpawn.Extensions;
using DotnetSpawn.Plugins;
using Json.Schema;

namespace DotnetSpawn.Templating.Schema
{
    internal class BoolInputJsonSchemaGenerator : ISpecificInputJsonSchemaGenerator
    {
        public int Precedence => 10;

        public bool CanGenerateSchemaOf(SpawnPointMetadata.InputMetadata input)
        {
            return
                input.Type == typeof(bool) ||
                input.Type == typeof(bool?);
        }

        public JsonSchema GenerateSchemaOf(
            SpawnPointMetadata.InputMetadata input,
            InputJsonSchemaGenerator generator)
        {
            var schemaType = SchemaValueType.Boolean | SchemaValueType.String;

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
