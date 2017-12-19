using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RockyToy.Common;

namespace RockyToy.MathParser
{
	internal interface IToken
	{
		string Expr { get; }
		TokenType Type { get; }
		IList<IToken> Children { get; }
	}

	internal class Token : IToken
	{
		public Token([NotNull] string expr, TokenType type)
		{
			ArgChk.NotNull(expr, nameof(expr));
			Expr = expr;
			Type = type;
			Children = null;
		}

		public Token([NotNull] string expr, IEnumerable<IToken> tokens, TokenType type)
		{
			ArgChk.NotNull(expr, nameof(expr));
			Expr = expr;
			Type = type;
			Children = tokens?.ToList();
		}

		public Token(IEnumerable<IToken> tokens, TokenType type)
		{
			Expr = string.Empty;
			Type = type;
			Children = tokens?.ToList();
		}

		[NotNull]
		public string Expr { get; }

		public TokenType Type { get; }
		public IList<IToken> Children { get; }
	}
}