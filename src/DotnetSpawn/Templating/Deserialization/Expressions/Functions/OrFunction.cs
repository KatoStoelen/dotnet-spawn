using DotnetSpawn.Templating.Deserialization.Expressions.Functions.Metadata;

namespace DotnetSpawn.Templating.Deserialization.Expressions.Functions
{
    internal class OrFunction : Function<bool>
    {
        private OrFunction()
        {
            MinimumArgumentCount = 2;
        }

        protected override object Invoke(bool[] args)
        {
            return args.Any(arg => arg is true);
        }

        public class Metadata : FunctionMetadata
        {
            public override string Name => "or";

            public override string Description =>
                "Returns [teal]true[/] if any of the arguments are [teal]true[/].";

            public override Type ReturnType => typeof(bool);

            public override IReadOnlyList<ParameterMetadata> Parameters => new[]
            {
                new ParameterMetadata
                {
                    Name = "boolValue",
                    Type = typeof(bool),
                    Description = "The values to evaluate.",
                    IsParams = true
                }
            };

            public override IEnumerable<string> Examples => new[]
            {
                "or(output('step1', 'output1'), output('step2', 'output2'))",
                "or(myBoolVar, output('step1', 'output1'))"
            };

            public override Func<Function> CreateInstance => () => new OrFunction();
        }
    }
}
