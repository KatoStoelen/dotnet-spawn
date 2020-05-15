using System.Globalization;
using DotnetSpawn.Templating.Deserialization.Expressions.Functions.Metadata;

namespace DotnetSpawn.Templating.Deserialization.Expressions.Functions
{
    internal class FormatFunction : Function
    {
        private FormatFunction()
        {
            MinimumArgumentCount = 2;
        }

        protected override object Invoke(object[] args)
        {
            ThrowIfInvalidArgs(args);

            var format = (string)args[0];
            var arguments = args.Skip(1).ToArray();

            return string.Format(CultureInfo.InvariantCulture, format, args: arguments);
        }

        private void ThrowIfInvalidArgs(object[] args)
        {
            if (args[0] is not string)
            {
                throw InvalidArgument(
                    $"Expected a string (was {args[0].GetType().FullName})",
                    argumentIndex: 0);
            }
        }

        public class Metadata : FunctionMetadata
        {
            public override string Name => "format";

            public override string Description =>
                "Replaces format items in [green]format[/] with the string representation of " +
                "the corresponding [green]arg[/].";

            public override Type ReturnType => typeof(string);

            public override IReadOnlyList<ParameterMetadata> Parameters => new[]
            {
                new ParameterMetadata
                {
                    Name = "format",
                    Type = typeof(string),
                    Description = "A composite format string."
                },
                new ParameterMetadata
                {
                    Name = "arg",
                    Type = typeof(object),
                    Description = "Arguments to insert into [green]format[/].",
                    IsParams = true
                }
            };

            public override IEnumerable<string> Examples => new[]
            {
                "format('https://{0}:{1}/', output('step1', 'hostname'), 8080)",
                "format('{0:yyyyMMdd}', output('step1', 'timestamp'))"
            };

            public override Func<Function> CreateInstance => () => new FormatFunction();

            public override string Remarks =>
                "Invariant culture is used when producing the string representation of the " +
                "specified format arguments.";
        }
    }
}