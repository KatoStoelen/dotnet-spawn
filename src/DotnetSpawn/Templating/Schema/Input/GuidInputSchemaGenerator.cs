using DotnetSpawn.Extensions;
using DotnetSpawn.Plugins;
using Json.Schema;

namespace DotnetSpawn.Templating.Schema
{
    internal class GuidInputSchemaGenerator : ISpecificInputJsonSchemaGenerator
    {
        public int Precedence => 40;

        public bool CanGenerateSchemaOf(SpawnPointMetadata.InputMetadata input)
        {
            return
                input.Type == typeof(Guid) ||
                input.Type == typeof(Guid?);
        }

        public JsonSchema GenerateSchemaOf(
            SpawnPointMetadata.InputMetadata input,
            InputJsonSchemaGenerator generator)
        {
            var schemaType = SchemaValueType.String;

            if (input.Type.IsNullable())
            {
                schemaType |= SchemaValueType.Null;
            }

            return new JsonSchemaBuilder()
                .Type(schemaType)
                .Format(Formats.Uuid)
                .Description(input.Description);
        }
    }
}
