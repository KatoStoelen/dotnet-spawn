using System.Text.RegularExpressions;
using DotnetSpawn.Plugins;
using Json.Schema;

namespace DotnetSpawn.Templating.Schema
{
    internal class DictionaryInputSchemaGenerator : ISpecificInputJsonSchemaGenerator
    {
        public int Precedence => 90;

        public bool CanGenerateSchemaOf(SpawnPointMetadata.InputMetadata input)
        {
            if (input.KeyType == null)
            {
                return false;
            }

            return
                input.KeyType == typeof(string) ||
                input.KeyType.IsEnum;
        }

        public JsonSchema GenerateSchemaOf(
            SpawnPointMetadata.InputMetadata input,
            InputJsonSchemaGenerator generator)
        {
            var schema = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Description(input.Description);

            var valueSchema = generator.GenerateSchemaOf(
                new SpawnPointMetadata.InputMetadata(input.ElementType));

            if (input.KeyType.IsEnum)
            {
                schema
                    .Properties(
                        Enum
                            .GetValues(input.KeyType)
                            .Cast<object>()
                            .ToDictionary(@enum => @enum.ToString(), _ => valueSchema))
                    .AdditionalProperties(false);
            }
            else
            {
                schema
                    .PatternProperties((new Regex("^.*$"), valueSchema));
            }

            return schema;
        }
    }
}
