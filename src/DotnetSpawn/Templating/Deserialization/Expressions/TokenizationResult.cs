using System.Collections;

namespace DotnetSpawn.Templating.Deserialization.Expressions
{
    internal class TokenizationResult : IReadOnlyList<Token>
    {
        private readonly IReadOnlyList<Token> _tokens;

        public TokenizationResult(IReadOnlyList<Token> tokens)
        {
            _tokens = tokens;
        }

        public Token this[int index] => _tokens[index];
        public int Count => _tokens.Count;

        public IEnumerator<Token> GetEnumerator() => _tokens.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool IsFunctionIdentifier(int tokenIndex)
        {
            var token = this[tokenIndex];
            var lookaheadToken = tokenIndex < Count - 1
                ? this[tokenIndex + 1]
                : null;

            if (token.Type == Token.TokenType.Identifier &&
                lookaheadToken == Token.FunctionCallStartSeparator)
            {
                return true;
            }

            return false;
        }


        public bool IsVariableIdentifier(int tokenIndex)
        {
            var token = this[tokenIndex];
            var lookaheadToken = tokenIndex < Count - 1
                ? this[tokenIndex + 1]
                : null;

            if (token.Type == Token.TokenType.Identifier && (
                    lookaheadToken == Token.FunctionCallEndSeparator ||
                    lookaheadToken == Token.FunctionArgumentSeparator))
            {
                return true;
            }

            return false;
        }
    }
}
