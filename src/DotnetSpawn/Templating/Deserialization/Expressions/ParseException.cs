using System.Text;

namespace DotnetSpawn.Templating.Deserialization.Expressions
{
    internal class ParseException : Exception
    {
        private const char IndicatorStart = '\u2514';
        private const char IndicatorLine = '\u2500';
        private const char IndicatorEnd = '\u2518';
        private const char IndicatorSingleChar = '\u2191';

        public ParseException(string message, Token token)
            : base(BuildExceptionMessage(message, token))
        {
        }

        public static string BuildExceptionMessage(
            string message, Token token)
        {
            return new StringBuilder(message)
                .AppendLine()
                .AppendLine(token.Expression)
                .AppendLine(
                    new string(' ', token.StartIndex) + (
                        token.Value.Length <= 1
                            ? IndicatorSingleChar
                            : IndicatorStart +
                              new string(IndicatorLine, token.Value.Length - 2) +
                              IndicatorEnd
                        ))
                .ToString();
        }
    }
}
