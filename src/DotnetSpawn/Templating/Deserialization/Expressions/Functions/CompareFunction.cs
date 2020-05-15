using DotnetSpawn.Extensions;
using DotnetSpawn.Templating.Deserialization.Expressions.Functions.Metadata;

namespace DotnetSpawn.Templating.Deserialization.Expressions.Functions
{
    internal abstract class CompareFunction : Function
    {
        protected CompareFunction()
        {
            MinimumArgumentCount = 2;
            MaximumArgumentCount = 3;
        }

        protected int Compare(object[] args)
        {
            ThrowIfInvalidArgs(args);

            if (args.Any(arg => arg is string))
            {
                var stringArgs = ConvertArguments<string>(args.Take(2));

                var ignoreCase = args.Length != 3 || (bool)args[2];

                var comparer = ignoreCase
                    ? StringComparer.OrdinalIgnoreCase
                    : StringComparer.Ordinal;

                return comparer.Compare(stringArgs[0], stringArgs[1]);
            }
            else if (args.Any(arg => arg.GetType().IsFloatingPointType()))
            {
                var decimalArgs = ConvertArguments<decimal?>(args.Take(2));

                return Comparer<decimal?>.Default
                    .Compare(decimalArgs[0], decimalArgs[1]);
            }
            else
            {
                var longArgs = ConvertArguments<long?>(args.Take(2));

                return Comparer<long?>.Default
                    .Compare(longArgs[0], longArgs[1]);
            }
        }

        private void ThrowIfInvalidArgs(object[] args)
        {
            for (var i = 0; i < args.Length - 1; i++)
            {
                var argType = args[i].GetType();
                var isValid =
                    argType == typeof(string) ||
                    argType.IsIntegerType() ||
                    argType.IsFloatingPointType();

                if (!isValid)
                {
                    throw InvalidArgument(
                        $"Expected string or number (was {argType.FullName})",
                        argumentIndex: i);
                }
            }

            if (args.Length == 3 && args[2] is not bool)
            {
                throw InvalidArgument(
                    $"Expected boolean (was {args[2].GetType().FullName})",
                    argumentIndex: 2);
            }
        }

        public abstract class CompareFunctionMetadata : FunctionMetadata
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
                "If one of the arguments is a string, ordinal case-insensitive comparison " +
                "is used to compare the string argument with (a string representation of) the other. " +
                "To perform case-sensitive comparison, set [green]ignoreCase[/] to [teal]false[/].";
        }
    }
}
