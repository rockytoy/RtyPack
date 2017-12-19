using System.Collections.Generic;
using JetBrains.Annotations;

namespace RockyToy.MathParser
{
	internal interface ITokenFactory
	{
		IToken CreateToken(TokenType type, [NotNull] string expr, [NotNull] params IToken[] tokens);
		IToken CreateToken(TokenType type, [NotNull] string expr);
		IToken CreateToken(TokenType type, [NotNull] IToken token);
		IToken CreateToken(TokenType type, [NotNull] params IToken[] tokens);
		IToken CreateToken(TokenType type, [NotNull] IEnumerable<IToken> tokens);
	}

	internal class TokenFactory : ITokenFactory
	{
		public IToken CreateToken(TokenType type, string expr, params IToken[] tokens)
		{
			return new Token(expr, tokens, type);
		}

		public IToken CreateToken(TokenType type, string expr)
		{
			return new Token(expr, type);
		}

		public IToken CreateToken(TokenType type, IToken token)
		{
			return new Token(new[] { token }, type);
		}

		public IToken CreateToken(TokenType type, params IToken[] tokens)
		{
			return new Token(tokens, type);
		}

		public IToken CreateToken(TokenType type, IEnumerable<IToken> tokens)
		{
			return new Token(tokens, type);
		}
	}
}