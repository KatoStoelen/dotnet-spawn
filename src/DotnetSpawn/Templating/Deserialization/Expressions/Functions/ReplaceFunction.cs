using DotnetSpawn.Templating.Deserialization.Expressions.Functions.Metadata;

namespace DotnetSpawn.Templating.Deserialization.Expressions.Functions
{
    internal class ReplaceFunction : Function
    {
        private ReplaceFunction()
        {
            MinimumArgumentCount = 3;
            MaximumArgumentCount = 4;
        }

        protected override object Invoke(object[] args)
        {
            ThrowIfInvalidArgs(args);

            var stringArgs = ConvertArguments<string>(args.Take(3));

            var input = stringArgs[0];
            var oldValue = stringArgs[1];
            var newValue = stringArgs[2];
            var ignoreCase = args.Length != 4 || (bool)args[3];

            var comparison = ignoreCase
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            return input.Replace(oldValue, newValue, comparison);
        }

        private void ThrowIfInvalidArgs(object[] args)
        {
            if (args.Length == 4 && args[3] is not bool)
            {
                throw InvalidArgument(
                    $"Expected boolean (was {args[3].GetType().FullName})",
                    argumentIndex: 3);
            }
        }

        public class Metadata : FunctionMetadata
        {
            public override string Name => "replace";

            public override string Description =>
                "Replases all occurrances of [green]oldValue[/] in [green]input[/] with " +
                "[green]newValue[/].";

            public override Type ReturnType => typeof(string);

            public override IReadOnlyList<ParameterMetadata> Parameters => new[]
            {
                new ParameterMetadata
                {
                    Name = "input",
                    Type = typeof(string),
                    Description = "The target for replacement."
                },
                new ParameterMetadata
                {
                    Name = "oldValue",
                    Type = typeof(string),
                    Description = "The string to be replaced."
                },
                new ParameterMetadata
                {
                    Name = "newValue",
                    Type = typeof(string),
                    Description = "The string to replace all occurrances of [green]oldValue[/]."
                },
                new ParameterMetadata
                {
                    Name = "ignoreCase",
                    Type = typeof(bool),
                    Description = "Whether or not to ignore case.",
                    IsOptional = true,
                    DefaultValue = true
                }
            };

            public override IEnumerable<string> Examples => new[]
            {
                "replace(output('step1', 'url'), 'http://', 'https://')",
                "replace(output('step1', 'name'), 'ABC', 'XYZ', false)"
            };

            public override Func<Function> CreateInstance => () => new ReplaceFunction();

            public sealed override string Remarks =>
                "Ordinal case-insensitive comparison is used by default. Setting [green]ignoreCase[/] " +
                "to [teal]false[/] results in ordinal case-sensitive comparison.";
        }
    }
}