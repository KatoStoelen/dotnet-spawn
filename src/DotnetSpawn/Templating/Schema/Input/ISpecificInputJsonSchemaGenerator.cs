using DotnetSpawn.Plugins;
using Json.Schema;

namespace DotnetSpawn.Templating.Schema
{
    internal interface ISpecificInputJsonSchemaGenerator
    {
        int Precedence { get; }

        bool CanGenerateSchemaOf(SpawnPointMetadata.InputMetadata input);
        JsonSchema GenerateSchemaOf(
            SpawnPointMetadata.InputMetadata input,
            InputJsonSchemaGenerator generator);
    }
}