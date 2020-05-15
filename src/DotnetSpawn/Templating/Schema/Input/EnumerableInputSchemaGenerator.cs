using System.Collections;
using DotnetSpawn.Plugins;
using Json.Schema;

namespace DotnetSpawn.Templating.Schema
{
    internal class EnumerableInputSchemaGenerator : ISpecificInputJsonSchemaGenerator
    {
        public int Precedence => 100;

        public bool CanGenerateSchemaOf(SpawnPointMetadata.InputMetadata input)
        {
            return input.Type.IsArray || input.Type.IsAssignableTo(typeof(IEnumerable));
        }

        public JsonSchema GenerateSchemaOf(
            SpawnPointMetadata.InputMetadata input,
            InputJsonSchemaGenerator generator)
        {
            return new JsonSchemaBuilder()
                .Type(SchemaValueType.Array | SchemaValueType.String)
                .Description(input.Description)
                .Items(
                    generator
                        .GenerateSchemaOf(new SpawnPointMetadata.InputMetadata(input.ElementType)));
        }
    }
}
