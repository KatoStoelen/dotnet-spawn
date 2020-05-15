using DotnetSpawn.Templating.Deserialization.Expressions.Functions.Metadata;

namespace DotnetSpawn.Templating.Deserialization.Expressions.Functions
{
    internal class NotFunction : Function<bool>
    {
        private NotFunction()
        {
            MinimumArgumentCount = 1;
            MaximumArgumentCount = 1;
        }

        protected override object Invoke(bool[] args)
        {
            var condition = args.Single();

            return condition is false;
        }

        public class Metadata : FunctionMetadata
        {
            public override string Name => "not";

            public override string Description =>
                "Returns [teal]true[/] if [green]condition[/] is [teal]false[/].";

            public override Type ReturnType => typeof(bool);

            public override IReadOnlyList<ParameterMetadata> Parameters => new[]
            {
                new ParameterMetadata
                {
                    Name = "condition",
                    Type = typeof(bool),
                    Description = "The condition to evaluate."
                }
            };

            public override IEnumerable<string> Examples => new[]
            {
                "not(output('step1', 'boolValue'))",
                "not(myBoolVar)"
            };

            public override Func<Function> CreateInstance => () => new NotFunction();
        }
    }
}