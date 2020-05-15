using DotnetSpawn.Templating.Deserialization.Expressions.Functions.Metadata;

namespace DotnetSpawn.Templating.Deserialization.Expressions.Functions
{
    internal class ToUpperFunction : Function<string>
    {
        private ToUpperFunction()
        {
            MinimumArgumentCount = 1;
            MaximumArgumentCount = 1;
        }

        protected override object Invoke(string[] args)
        {
            return args.Single().ToUpperInvariant();
        }

        public class Metadata : FunctionMetadata
        {
            public override string Name => "toUpper";

            public override string Description =>
                "Converts the specified argument to upper case.";

            public override Type ReturnType => typeof(string);

            public override IReadOnlyList<ParameterMetadata> Parameters => new[]
            {
                new ParameterMetadata
                {
                    Name = "value",
                    Description = "The value to convert to upper case.",
                    Type = typeof(string)
                }
            };

            public override IEnumerable<string> Examples => new[]
            {
                "toUpper('abc')",
                "toUpper(output('step1', 'output1'))"
            };

            public override Func<Function> CreateInstance => () => new ToUpperFunction();

            public override string Remarks =>
                "Invariant culture is used when transforming to upper case.";
        }
    }
}
