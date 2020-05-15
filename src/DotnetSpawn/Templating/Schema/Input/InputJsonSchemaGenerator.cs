using DotnetSpawn.IO;
using DotnetSpawn.Plugins;
using Json.Schema;

namespace DotnetSpawn.Templating.Schema
{
    internal class InputJsonSchemaGenerator
    {
        private readonly IEnumerable<ISpecificInputJsonSchemaGenerator> _specificGenerators;
        private readonly SpectreConsole _console;

        public InputJsonSchemaGenerator(
            IEnumerable<ISpecificInputJsonSchemaGenerator> specificGenerators,
            SpectreConsole console)
        {
            _specificGenerators = specificGenerators;
            _console = console;
        }

        public JsonSchema GenerateSchemaOf(SpawnPointMetadata.InputMetadata input)
        {
            var generator = _specificGenerators
                .OrderBy(generator => generator.Precedence)
                .FirstOrDefault(generator => generator.CanGenerateSchemaOf(input));

            if (generator == null)
            {
                throw new InvalidOperationException(
                    $"Could not determine JSON schema generator of input.Type '{input.Type}'");
            }

            _console.LogDebug($"Using generator {generator.GetType().Name} for input {input}");

            return generator.GenerateSchemaOf(input, this);
        }
    }
}
