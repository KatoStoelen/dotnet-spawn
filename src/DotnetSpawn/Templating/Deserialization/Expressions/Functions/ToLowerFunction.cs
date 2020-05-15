using DotnetSpawn.Templating.Deserialization.Expressions.Functions.Metadata;

namespace DotnetSpawn.Templating.Deserialization.Expressions.Functions
{
    internal class ToLowerFunction : Function<string>
    {
        private ToLowerFunction()
        {
            MinimumArgumentCount = 1;
            MaximumArgumentCount = 1;
        }

        protected override object Invoke(string[] args)
        {
            return args.Single().ToLowerInvariant();
        }

        public class Metadata : FunctionMetadata
        {
            public override string Name => "toLower";

            public override string Description =>
                "Converts the specified argument to lower case.";

            public override Type ReturnType => typeof(string);

            public override IReadOnlyList<ParameterMetadata> Parameters => new[]
            {
                new ParameterMetadata
                {
                    Name = "value",
                    Description = "The value to convert to lower case.",
                    Type = typeof(string)
                }
            };

            public override IEnumerable<string> Examples => new[]
            {
                "toLower('ABC')",
                "toLower(output('step1', 'output1'))"
            };

            public override Func<Function> CreateInstance => () => new ToLowerFunction();

            public override string Remarks =>
                "Invariant culture is used when transforming to lower case.";
        }
    }
}
