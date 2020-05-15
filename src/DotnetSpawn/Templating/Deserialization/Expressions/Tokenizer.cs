namespace DotnetSpawn.Templating.Deserialization.Expressions
{
    internal static class Tokenizer
    {
        public static TokenizationResult Tokenize(string expression)
        {
            var tokens = new List<Token>();

            var tokenValue = string.Empty;
            var tokenValueStartIndex = 0;
            var isStringLiteralContext = false;

            for (var i = 0; i < expression.Length; i++)
            {
                var @char = expression[i];

                if (isStringLiteralContext)
                {
                    tokenValue += @char;

                    if (Token.IsStringLiteralDelimiter(@char))
                    {
                        tokens.Add(new Token.StringLiteral(
                            tokenValue, tokenValueStartIndex, expression));

                        Advance(currentIndex: i);
                    }
                }
                else if (Token.IsStringLiteralDelimiter(@char))
                {
                    tokenValue += @char;
                    isStringLiteralContext = true;
                }
                else if (Token.IsSeparator(@char))
                {
                    if (tokenValue.Length > 0)
                    {
                        tokens.Add(GetToken());
                    }

                    tokens.Add(new Token.Separator(@char, tokenValueStartIndex, expression));

                    Advance(currentIndex: i);
                }
                else if (char.IsWhiteSpace(@char))
                {
                    if (tokenValue.Length == 0)
                    {
                        tokenValueStartIndex++;
                    }
                    else
                    {
                        tokens.Add(GetToken());
                        Advance(currentIndex: i);
                    }
                }
                else
                {
                    tokenValue += @char;
                }
            }

            if (isStringLiteralContext)
            {
                var invalidStringLiteralToken = new Token.StringLiteral(
                    tokenValue, tokenValueStartIndex, expression);

                throw new ParseException(
                    "String literal not terminated", invalidStringLiteralToken);
            }
            else if (tokenValue.Length > 0)
            {
                tokens.Add(GetToken());
            }

            return new TokenizationResult(tokens);

            void Advance(int currentIndex)
            {
                tokenValue = string.Empty;
                tokenValueStartIndex = currentIndex + 1;
                isStringLiteralContext = false;
            }

            Token GetToken()
            {
                return tokenValue switch
                {
                    var value when Token.IsBooleanLiteral(value) =>
                        new Token.BooleanLiteral(value, tokenValueStartIndex, expression),

                    var value when Token.IsIdentifier(value) =>
                        new Token.Identifier(value, tokenValueStartIndex, expression),

                    var value when Token.IsNumberLiteral(value) =>
                        new Token.NumberLiteral(value, tokenValueStartIndex, expression),

                    _ => (Token)new Token.Unknown(tokenValue, tokenValueStartIndex, expression)
                };
            }
        }
    }
}
