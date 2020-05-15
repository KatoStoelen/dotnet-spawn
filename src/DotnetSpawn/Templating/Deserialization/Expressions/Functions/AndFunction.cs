using DotnetSpawn.Templating.Deserialization.Expressions.Functions.Metadata;

namespace DotnetSpawn.Templating.Deserialization.Expressions.Functions
{
    internal class AndFunction : Function<bool>
    {
        private AndFunction()
        {
            MinimumArgumentCount = 2;
        }

        protected override object Invoke(bool[] args)
        {
            return args.All(arg => arg is true);
        }

        public class Metadata : FunctionMetadata
        {
            public override string Name => "and";

            public override string Description =>
                "Returns [teal]true[/] if all the arguments are [teal]true[/].";

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
                "and(output('step1', 'output1'), output('step2', 'output2'))",
                "and(myBoolVar, output('step1', 'output1'))"
            };

            public override Func<Function> CreateInstance => () => new AndFunction();
        }
    }
}