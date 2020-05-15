using System.Text;
using DotnetSpawn.Templating.Deserialization.Expressions.Functions.Metadata;

namespace DotnetSpawn.Templating.Deserialization.Expressions.Functions
{
    internal abstract class EqualityFunction : Function
    {
        protected EqualityFunction()
        {
            MinimumArgumentCount = 2;
            MaximumArgumentCount = 3;
        }

        protected bool IsEqual(object[] args)
        {
            ThrowIfInvalidArgs(args);

            if (args.Any(arg => arg is string))
            {
                var stringArgs = ConvertArguments<string>(args.Take(2));

                var ignoreCase = args.Length != 3 || (bool)args[2];

                var comparer = ignoreCase
                    ? StringComparer.OrdinalIgnoreCase
                    : StringComparer.Ordinal;

                return comparer.Compare(stringArgs[0], stringArgs[1]) == 0;
            }

            var first = args[0];
            var other = args[1];

            return first is null
                ? other is null
                : first.Equals(other);
        }

        private void ThrowIfInvalidArgs(object[] args)
        {
            if (args.Length == 3 && args[2] is not bool)
            {
                throw InvalidArgument(
                    $"Expected boolean (was {args[2].GetType().FullName})",
                    argumentIndex: 2);
            }
        }

        public abstract class EqualityFunctionMetadata : FunctionMetadata
        {
            public sealed override Type ReturnType => typeof(bool);

            public override IReadOnlyList<ParameterMetadata> Parameters => new[]
            {
                new ParameterMetadata
                {
                    Name = "first",
                    Description = "The first value to compare.",
                    Type = typeof(object)
                },
                new ParameterMetadata
                {
                    Name = "second",
                    Description = "The second value to compare.",
                    Type = typeof(object)
                },
                new ParameterMetadata
                {
                    Name = "ignoreCase",
                    Description = "Whether or not to ignore case. Only applicable for string comparison.",
                    Type = typeof(bool),
                    IsOptional = true,
                    DefaultValue = true
                }
            };

            public override string Remarks =>
                new StringBuilder()
                    .AppendLine(
                        "If one of the arguments is a string, ordinal case-insensitive comparison " +
                        "is used to compare the string argument with (a string representation of) the other. " +
                        "Otherwise, equality is checked using [bold]first.Equals(other)[/].")
                    .AppendLine()
                    .Append(
                        "When comparing strings, setting the third argument to [teal]false[/] results in " +
                        "ordinal case-sensitive comparison.")
                    .ToString();
        }
    }
}
