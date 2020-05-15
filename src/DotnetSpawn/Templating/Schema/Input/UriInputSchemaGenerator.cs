using DotnetSpawn.Plugins;
using Json.Schema;

namespace DotnetSpawn.Templating.Schema
{
    internal class UriInputSchemaGenerator : ISpecificInputJsonSchemaGenerator
    {
        public int Precedence => 60;

        public bool CanGenerateSchemaOf(SpawnPointMetadata.InputMetadata input)
        {
            return input.Type == typeof(Uri);
        }

        public JsonSchema GenerateSchemaOf(
            SpawnPointMetadata.InputMetadata input,
            InputJsonSchemaGenerator generator)
        {
            return new JsonSchemaBuilder()
                .Type(SchemaValueType.String)
                .Format(Formats.Uri)
                .Description(input.Description);
        }
    }
}
