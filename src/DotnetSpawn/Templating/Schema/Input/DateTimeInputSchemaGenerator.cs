using DotnetSpawn.Extensions;
using DotnetSpawn.Plugins;
using Json.Schema;

namespace DotnetSpawn.Templating.Schema
{
    internal class DateTimeInputSchemaGenerator : ISpecificInputJsonSchemaGenerator
    {
        public int Precedence => 50;

        public bool CanGenerateSchemaOf(SpawnPointMetadata.InputMetadata input)
        {
            return
                input.Type == typeof(DateTime) ||
                input.Type == typeof(DateTime?);
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
                .Format(Formats.DateTime)
                .Description(input.Description);
        }
    }
}
