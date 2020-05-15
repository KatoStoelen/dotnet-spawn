using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DotnetSpawn.Templating.Deserialization.Expressions
{
    [DebuggerDisplay($"{{{nameof(ToString)}(),nq}}")]
    internal class Token : IEquatable<Token>
    {
        private static readonly char[] s_separators = new[] { '(', ')', ',' };

        public static readonly Token FunctionCallStartSeparator = new Separator('(');
        public static readonly Token FunctionCallEndSeparator = new Separator(')');
        public static readonly Token FunctionArgumentSeparator = new Separator(',');

        protected Token(string value, TokenType type, int startIndex, string expression)
        {
            Value = value;
            Type = type;
            StartIndex = startIndex;
            Expression = expression;
        }

        public string Value { get; }
        public TokenType Type { get; }
        public int StartIndex { get; }
        public string Expression { get; }

        public override bool Equals(object obj)
        {
            return
                ReferenceEquals(this, obj) ||
                    obj is Token other && Equals(other)
                ;
        }

        public bool Equals(Token other)
        {
            return
                other != null &&
                Type == other.Type &&
                Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value, Type);
        }

        public override string ToString() => $"{Type}, {Value}";

        public static bool IsIdentifier(string input)
        {
            return Regex.IsMatch(input, @"^[a-zA-Z]\w*$");
        }

        public static bool IsSeparator(char input)
        {
            return s_separators.Contains(input);
        }

        public static bool IsStringLiteralDelimiter(char input)
        {
            return input == '\'';
        }

        public static bool IsBooleanLiteral(string input)
        {
            return
                "true".Equals(input, StringComparison.OrdinalIgnoreCase) ||
                "false".Equals(input, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsNumberLiteral(string input)
        {
            return Regex.IsMatch(input, $@"^-?(\d+(\.\d+)?|\.\d+)$");
        }

        public static bool operator ==(Token first, Token other) =>
            first is null
                ? other is null
                : first.Equals(other);

        public static bool operator !=(Token first, Token other) => !(first == other);

        public enum TokenType
        {
            Unknown = 0,
            Keyword = 1,
            Identifier = 2,
            Separator = 3,
            StringLiteral = 4,
            NumberLiteral = 5,
            BooleanLiteral = 6
        }

        public class Unknown : Token
        {
            public Unknown(string value, int startIndex, string expression)
                : base(value, TokenType.Unknown, startIndex, expression)
            {
            }
        }

        public class Keyword : Token
        {
            public Keyword(string value, int startIndex, string expression)
                : base(value, TokenType.Keyword, startIndex, expression)
            {
            }
        }

        public class Identifier : Token
        {
            public Identifier(string value, int startIndex, string expression)
                : base(value, TokenType.Identifier, startIndex, expression)
            {
            }
        }

        public class Separator : Token
        {
            public Separator(char value, int startIndex, string expression)
                : base(value.ToString(), TokenType.Separator, startIndex, expression)
            {
            }

            public Separator(char value)
                : this(value, -1, string.Empty)
            {
            }
        }

        public class StringLiteral : Token
        {
            public StringLiteral(string value, int startIndex, string expression)
                : base(value, TokenType.StringLiteral, startIndex, expression)
            {
            }

            public string UnquotedValue => Value[1..^1];
        }

        public class NumberLiteral : Token
        {
            public NumberLiteral(string value, int startIndex, string expression)
                : base(value, TokenType.NumberLiteral, startIndex, expression)
            {
            }

            public bool IsFloatingPoint => Value.Contains('.');
        }

        public class BooleanLiteral : Token
        {
            public BooleanLiteral(string value, int startIndex, string expression)
                : base(value, TokenType.BooleanLiteral, startIndex, expression)
            {
            }

            public bool BoolValue => bool.Parse(Value);
        }
    }
}