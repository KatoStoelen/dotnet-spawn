using DotnetSpawn.Extensions;

namespace DotnetSpawn.Templating.Deserialization.Expressions.Functions
{
    internal abstract class Function
    {
        private IReadOnlyList<Token> _argumentTokens;

        public int MinimumArgumentCount { get; protected set; }
        public int? MaximumArgumentCount { get; protected set; }
        public bool IsContextual { get; protected set; }

        public object Invoke(IReadOnlyList<Token> tokens, object[] args)
        {
            _argumentTokens = tokens;

            return Invoke(args);
        }

        protected abstract object Invoke(object[] args);

        protected TArgs[] ConvertArguments<TArgs>(IEnumerable<object> args)
        {
            if (typeof(TArgs) == typeof(string))
            {
                return args
                    .Select(arg => arg.ConvertToString())
                    .Cast<TArgs>()
                    .ToArray();
            }

            return args
                .Select((arg, index) =>
                {
                    try
                    {
                        return arg.ConvertTo<TArgs>();
                    }
                    catch (Exception)
                    {
                        throw InvalidArgument(
                            $"Could not convert {arg.GetType().FullName} to {typeof(TArgs).FullName}",
                            argumentIndex: index);
                    }
                })
                .ToArray();
        }

        protected Exception InvalidArgument(string message, int argumentIndex)
        {
            if (argumentIndex < 0 || argumentIndex > _argumentTokens.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(argumentIndex));
            }

            return new ParseException(message, _argumentTokens[argumentIndex]);
        }
    }

    internal abstract class Function<TArgs> : Function
    {
        protected sealed override object Invoke(object[] args)
        {
            return Invoke(ConvertArguments<TArgs>(args));
        }

        protected abstract object Invoke(TArgs[] args);
    }
}