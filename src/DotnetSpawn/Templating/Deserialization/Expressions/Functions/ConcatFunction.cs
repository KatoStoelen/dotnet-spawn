using DotnetSpawn.Templating.Deserialization.Expressions.Functions.Metadata;

namespace DotnetSpawn.Templating.Deserialization.Expressions.Functions
{
    internal class ConcatFunction : Function<string>
    {
        private ConcatFunction()
        {
            MinimumArgumentCount = 2;
        }

        protected override object Invoke(string[] args)
        {
            return string.Concat(args);
        }

        public class Metadata : FunctionMetadata
        {
            public override string Name => "concat";

            public override string Description =>
                "Concatenates the string representation of the specified arguments.";

            public override Type ReturnType => typeof(string);

            public override IReadOnlyList<ParameterMetadata> Parameters => new[]
            {
                new ParameterMetadata
                {
                    Name = "value",
                    Description = "The values to concatenate.",
                    Type = typeof(string),
                    IsParams = true
                }
            };

            public override IEnumerable<string> Examples => new[]
            {
                "concat('prefix-', output('step1', 'output1'), '-suffix')"
            };

            public override Func<Function> CreateInstance => () => new ConcatFunction();

            public override string Remarks =>
                "The string representation of any non-string argument is obtained using " +
                "[bold]object.ToString()[/] (with invariant culture where applicable).";
        }
    }
}
