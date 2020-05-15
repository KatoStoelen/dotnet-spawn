using DotnetSpawn.Templating.Deserialization.Expressions.Functions.Metadata;

namespace DotnetSpawn.Templating.Deserialization.Expressions.Functions
{
    internal class UniqueIdFunction : Function<int>
    {
        private UniqueIdFunction()
        {
            MinimumArgumentCount = 0;
            MaximumArgumentCount = 1;
        }

        protected override object Invoke(int[] args)
        {
            var length = args.Length == 0 ? 13 : args.Single();

            var uniqueId = string.Empty;

            while (uniqueId.Length < length)
            {
                uniqueId += Guid.NewGuid().ToString("N");
            }

            return uniqueId[..length];
        }

        public class Metadata : FunctionMetadata
        {
            public override string Name => "uniqueId";

            public override string Description => "Generates a unique ID.";

            public override Type ReturnType => typeof(string);

            public override IReadOnlyList<ParameterMetadata> Parameters => new[]
            {
                new ParameterMetadata
                {
                    Name = "length",
                    Description = "The length of the unique ID.",
                    Type = typeof(int),
                    IsOptional = true,
                    DefaultValue = 13
                }
            };

            public override IEnumerable<string> Examples => new[]
            {
                "uniqueId()",
                "uniqueId(42)"
            };

            public override Func<Function> CreateInstance => () => new UniqueIdFunction();
        }
    }
}
