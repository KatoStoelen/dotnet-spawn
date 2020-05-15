using System.Globalization;
using System.Linq.Expressions;
using DotnetSpawn.Templating.Deserialization.Expressions.Functions;
using DotnetSpawn.Templating.Execution;

namespace DotnetSpawn.Templating.Deserialization.Expressions
{
    internal static class ExpressionParser
    {
        private static readonly Token.TokenType[] s_argumentTokenTypes = new[]
        {
            Token.TokenType.Identifier,
            Token.TokenType.StringLiteral,
            Token.TokenType.NumberLiteral
        };

        public static Expression<Func<IExecutionContext, object>> Parse(
            TokenizationResult tokenResult)
        {
            var tokenOffset = 0;
            var contextParameter = Expression.Parameter(typeof(IExecutionContext), "context");

            var lambdaBody = tokenOffset switch
            {
                var offset when tokenResult.IsFunctionIdentifier(offset) =>
                    ParseFunction(ref tokenOffset, tokenResult, contextParameter),

                var offset when tokenResult.IsVariableIdentifier(offset) =>
                    ParseVariableReference((Token.Identifier)tokenResult[offset], contextParameter),

                _ => throw new ParseException(
                    "Expected function or variable identifier", tokenResult[tokenOffset])
            };

            return Expression
                .Lambda<Func<IExecutionContext, object>>(lambdaBody, contextParameter);
        }

        private static Expression ParseFunction(
            ref int tokenOffset, TokenizationResult tokenResult, ParameterExpression context)
        {
            var currentToken = tokenResult[tokenOffset];

            if (!tokenResult.IsFunctionIdentifier(tokenOffset))
            {
                throw new ParseException("Expected function call expression", currentToken);
            }

            if (!FunctionRegistry.TryGet(currentToken.Value, out var function))
            {
                throw new ParseException("Unknown function", currentToken);
            }

            tokenOffset += 2;

            var arguments = new List<Expression>();
            var argumentTokens = new List<Token>();

            if (function.IsContextual)
            {
                arguments.Add(context);
            }

            foreach (var (argument, token) in ParseArguments(ref tokenOffset, tokenResult, function, context))
            {
                arguments.Add(argument);
                argumentTokens.Add(token);
            }

            var argsExpression = Expression.NewArrayInit(typeof(object), arguments);

            return Expression.Call(
                Expression.Constant(function),
                function.GetType().GetMethod(nameof(Function.Invoke)),
                Expression.Constant(argumentTokens),
                argsExpression);
        }

        private static IReadOnlyList<(Expression, Token)> ParseArguments(
            ref int tokenOffset,
            TokenizationResult tokenResult,
            Function function,
            ParameterExpression context)
        {
            var arguments = new List<(Expression, Token)>();
            var functionIdentifierOffset = tokenOffset - 2;

            while (tokenResult[tokenOffset] != Token.FunctionCallEndSeparator)
            {
                arguments.Add(ParseArgument(ref tokenOffset, tokenResult, context));

                tokenOffset++;

                if (tokenResult[tokenOffset] != Token.FunctionArgumentSeparator &&
                    tokenResult[tokenOffset] != Token.FunctionCallEndSeparator)
                {
                    throw new ParseException(
                        "Expected argument separator ',' or function call end ')'",
                        tokenResult[tokenOffset]);
                }

                if (tokenResult[tokenOffset] == Token.FunctionArgumentSeparator)
                {
                    tokenOffset++;

                    if (tokenResult[tokenOffset] == Token.FunctionCallEndSeparator)
                    {
                        throw new ParseException(
                            "Missing argument or misplaced argument separator",
                            tokenResult[tokenOffset - 1]);
                    }
                }
            }

            if (arguments.Count < function.MinimumArgumentCount)
            {
                throw new ParseException(
                    $"Function requires minimum {function.MinimumArgumentCount} arguments (# args was {arguments.Count})",
                    tokenResult[functionIdentifierOffset]);
            }

            if (arguments.Count > function.MaximumArgumentCount)
            {
                throw new ParseException(
                    $"Function has maximum {function.MaximumArgumentCount} arguments (# args was {arguments.Count})",
                    tokenResult[functionIdentifierOffset]);
            }

            return arguments;
        }

        private static (Expression, Token) ParseArgument(
            ref int tokenOffset, TokenizationResult tokenResult, ParameterExpression context)
        {
            var currentToken = tokenResult[tokenOffset];

            if (!s_argumentTokenTypes.Contains(currentToken.Type))
            {
                throw new ParseException(
                    $"Expected {string.Join(", ", s_argumentTokenTypes)} (was {currentToken.Type})",
                    currentToken);
            }

            return currentToken switch
            {
                Token.StringLiteral literal => (ParseStringLiteral(literal), literal),
                Token.NumberLiteral literal => (ParseNumberLiteral(literal), literal),
                Token.BooleanLiteral literal => (ParseBooleanLiteral(literal), literal),

                Token.Identifier identifier when tokenResult.IsFunctionIdentifier(tokenOffset) =>
                    (ParseFunction(ref tokenOffset, tokenResult, context), identifier),

                Token.Identifier identifier when tokenResult.IsVariableIdentifier(tokenOffset) =>
                    (ParseVariableReference(identifier, context), identifier),

                _ => throw new NotSupportedException()
            };
        }

        private static Expression ParseStringLiteral(Token.StringLiteral literal)
        {
            return Expression.Constant(literal.UnquotedValue);
        }

        private static Expression ParseNumberLiteral(Token.NumberLiteral literal)
        {
            Expression expression;

            if (literal.IsFloatingPoint)
            {
                expression = Expression.Constant(
                    double.Parse(literal.Value, NumberStyles.Number, CultureInfo.InvariantCulture));
            }
            else
            {
                expression = Expression.Constant(
                    long.Parse(literal.Value, NumberStyles.Integer, CultureInfo.InvariantCulture));
            }

            return Expression.Convert(expression, typeof(object));
        }

        private static Expression ParseBooleanLiteral(Token.BooleanLiteral literal)
        {
            return Expression.Constant(literal.BoolValue);
        }

        private static Expression ParseVariableReference(
            Token.Identifier variableIdentifier, ParameterExpression context)
        {
            Expression<Func<IExecutionContext, string, object>> getVariable =
                (context, variable) => context.Variable.Get(variable);

            return Expression.Invoke(
                getVariable,
                context,
                Expression.Constant(variableIdentifier.Value));
        }
    }
}