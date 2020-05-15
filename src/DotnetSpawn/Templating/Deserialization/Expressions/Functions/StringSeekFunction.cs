using DotnetSpawn.Templating.Deserialization.Expressions.Functions.Metadata;

namespace DotnetSpawn.Templating.Deserialization.Expressions.Functions
{
    internal abstract class StringSeekFunction : Function
    {
        protected StringSeekFunction()
        {
            MinimumArgumentCount = 2;
            MaximumArgumentCount = 3;
        }

        protected sealed override object Invoke(object[] args)
        {
            ThrowIfInvalidArgs(args);

            var stringArgs = ConvertArguments<string>(args.Take(2));
            var ignoreCase = args.Length != 3 || (bool)args[2];

            var input = stringArgs[0];
            var value = stringArgs[1];

            var comparison = ignoreCase
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            return Seek(input, value, comparison);
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

        protected abstract bool Seek(string input, string value, StringComparison comparison);

        public abstract class StringSeekFunctionMetadata : FunctionMetadata
        {
            public sealed override Type ReturnType => typeof(bool);

            public sealed override IReadOnlyList<ParameterMetadata> Parameters => new[]
            {
                new ParameterMetadata
                {
                    Name = "input",
                    Type = typeof(string),
                    Description = "The input string to evaluate."
                },
                new ParameterMetadata
                {
                    Name = "value",
                    Type = typeof(string),
                    Description = "The string to seek."
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

            public sealed override string Remarks =>
                "Ordinal case-insensitive comparison is used by default. Setting [green]ignoreCase[/] " +
                "to [teal]false[/] results in ordinal case-sensitive comparison.";
        }
    }
}
