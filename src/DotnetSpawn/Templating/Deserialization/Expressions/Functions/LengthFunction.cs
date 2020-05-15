using DotnetSpawn.Templating.Deserialization.Expressions.Functions.Metadata;

namespace DotnetSpawn.Templating.Deserialization.Expressions.Functions
{
    internal class LengthFunction : Function<string>
    {
        private LengthFunction()
        {
            MinimumArgumentCount = 1;
            MaximumArgumentCount = 1;
        }

        protected override object Invoke(string[] args)
        {
            var input = args.Single();

            return input.Length;
        }

        public class Metadata : FunctionMetadata
        {
            public override string Name => "length";

            public override string Description => "Returns the length of [green]input[/].";

            public override Type ReturnType => typeof(int);

            public override IReadOnlyList<ParameterMetadata> Parameters => new[]
            {
                new ParameterMetadata
                {
                    Name = "input",
                    Type = typeof(string),
                    Description = "The string to get length of."
                }
            };

            public override IEnumerable<string> Examples => new[]
            {
                "length(output('step1', 'name'))"
            };

            public override Func<Function> CreateInstance => () => new LengthFunction();
        }
    }
}