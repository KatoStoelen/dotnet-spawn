using System.Diagnostics;
using System.Text;
using DotnetSpawn.Extensions;

namespace DotnetSpawn.Templating.Deserialization.Expressions.Functions.Metadata
{
    [DebuggerDisplay($"{{{nameof(ToString)}(),nq}}")]
    internal abstract class FunctionMetadata
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract Type ReturnType { get; }
        public abstract IReadOnlyList<ParameterMetadata> Parameters { get; }
        public abstract IEnumerable<string> Examples { get; }
        public abstract Func<Function> CreateInstance { get; }
        public virtual string Remarks { get; } = string.Empty;

        public override string ToString()
        {
            return ToString("N");
        }

        public string ToString(string format)
        {
            return format switch
            {
                "N" => $"{Name} => {ReturnType.ToPrettyName()}",
                "I" => ToInvocationString(),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(format), format, $"Invalid format: {format}")
            };
        }

        private string ToInvocationString()
        {
            var builder = new StringBuilder(Name)
                .Append('(');

            var function = CreateInstance();

            for (var i = 0; i < Parameters.Count; i++)
            {
                var parameter = Parameters[i];

                if (parameter.IsParams)
                {
                    var requiredParamsCount = function.MinimumArgumentCount - i;

                    for (var paramsIndex = 1; paramsIndex <= requiredParamsCount; paramsIndex++)
                    {
                        builder.Append($"{parameter.Name}{paramsIndex}, ");
                    }

                    builder.Append('[');

                    if (requiredParamsCount <= 0)
                    {
                        builder.Append($"{parameter.Name}1, ");
                    }

                    builder.Append($"..., {parameter.Name}N]");

                    continue;
                }
                else if (parameter.IsOptional)
                {
                    builder.Append($"[{parameter.Name} = {parameter.DefaultValue.ConvertToString()}]");
                }
                else
                {
                    builder.Append(parameter.Name);
                }

                if (i < Parameters.Count - 1)
                {
                    builder.Append(", ");
                }
            }

            return builder
                .Append(')')
                .ToString();
        }
    }
}
