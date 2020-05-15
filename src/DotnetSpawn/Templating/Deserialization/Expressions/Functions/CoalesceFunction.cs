using DotnetSpawn.Templating.Deserialization.Expressions.Functions.Metadata;

namespace DotnetSpawn.Templating.Deserialization.Expressions.Functions
{
    internal class CoalesceFunction : Function
    {
        private CoalesceFunction()
        {
            MinimumArgumentCount = 2;
        }

        protected override object Invoke(object[] args)
        {
            return args.FirstOrDefault(arg => arg is not null);
        }

        public class Metadata : FunctionMetadata
        {
            public override string Name => "coalesce";

            public override string Description =>
                "Returns the first non-null value of the specified arguments.";

            public override Type ReturnType => typeof(object);

            public override IReadOnlyList<ParameterMetadata> Parameters => new[]
            {
                new ParameterMetadata
                {
                    Name = "value",
                    Description = "The values to coalesce.",
                    Type = typeof(object),
                    IsParams = true
                }
            };

            public override IEnumerable<string> Examples => new[]
            {
                "coalesce(output('step1', 'output1'), 'value_if_output_is_null')",
                "coalesce(output('step2', 'output2'), 1337)"
            };

            public override Func<Function> CreateInstance => () => new CoalesceFunction();
        }
    }
}
