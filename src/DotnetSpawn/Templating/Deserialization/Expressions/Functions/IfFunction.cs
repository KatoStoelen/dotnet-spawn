using DotnetSpawn.Templating.Deserialization.Expressions.Functions.Metadata;

namespace DotnetSpawn.Templating.Deserialization.Expressions.Functions
{
    internal class IfFunction : Function
    {
        private IfFunction()
        {
            MinimumArgumentCount = 3;
            MaximumArgumentCount = 3;
        }

        protected override object Invoke(object[] args)
        {
            var condition = args[0] as bool?;

            if (!condition.HasValue)
            {
                throw InvalidArgument(
                    $"Expected boolean (was {args[0].GetType().Name})",
                    argumentIndex: 0);
            }

            return condition.Value ? args[1] : args[2];
        }

        public class Metadata : FunctionMetadata
        {
            public override string Name => "if";

            public override string Description =>
                "Evaluates [green]condition[/] and returns [green]trueValue[/] if [teal]true[/], " +
                "otherwise [green]falseValue[/].";

            public override Type ReturnType => typeof(object);

            public override IReadOnlyList<ParameterMetadata> Parameters => new[]
            {
                new ParameterMetadata
                {
                    Name = "condition",
                    Description = "The condition to evaluate.",
                    Type = typeof(bool)
                },
                new ParameterMetadata
                {
                    Name = "trueValue",
                    Description = "The value to return if the condition is [teal]true[/].",
                    Type = typeof(object)
                },
                new ParameterMetadata
                {
                    Name = "falseValue",
                    Description = "The value to return if the condition is [teal]false[/].",
                    Type = typeof(object)
                }
            };

            public override IEnumerable<string> Examples => new[]
            {
                "if(output('step1', 'output1'), 'true', 'false')",
                "if(output('step1', 'output1'), output('step1', 'output2'), 1337)"
            };

            public override Func<Function> CreateInstance => () => new IfFunction();
        }
    }
}
