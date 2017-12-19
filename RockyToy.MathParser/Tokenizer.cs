using System.Diagnostics;
using System.Linq;
using Sprache;

namespace RockyToy.MathParser
{
	internal interface ITokenizer
	{
		IToken Tokenize(string expr);
	}

	internal class Tokenizer : ITokenizer
	{
		protected readonly ITokenFactory TokenFactory;
		public Tokenizer(ITokenFactory factory)
		{
			TokenFactory = factory;
		}

		#region Number

		// \.(\d+)
		protected virtual Parser<string> NumberFracString
		{
			get
			{
				Debug.WriteLine($"parsing {nameof(NumberFracString)}");
				return Parse.Char('.').Once().Then(dot => Parse.Digit.AtLeastOnce().Select(frac => string.Concat(dot.Union(frac))));
			}
		}

		// (eE)(\+\-)?(\d+)
		protected virtual Parser<string> NumberExpString
		{
			get
			{
				Debug.WriteLine($"parsing {nameof(NumberExpString)}");
				return Parse.Chars("Ee").Once()
					.Then(e => Parse.Digit.XAtLeastOnce().Select(n => string.Concat(e.Union(n))).XOr(Parse
						.Chars("+-").Once()
						.Then(s => Parse.Digit.XAtLeastOnce().Select(n => string.Concat(e.Union(s).Union(n))))));
			}
		}

		// (\d+)
		protected virtual Parser<string> NumberIntString
		{
			get
			{
				Debug.WriteLine($"parsing {nameof(NumberIntString)}");
				return Parse.Digit.AtLeastOnce()
					.Select(string.Concat);
			}
		}
		
		// Number => (NumberInt)(NumberFrac)?(NumberExp)? | NumberFrac
		protected virtual Parser<string> NumberString
		{
			get
			{
				Debug.WriteLine($"parsing {nameof(NumberString)}");
				return NumberIntString.Then(integer => NumberFracString.Then(frac => NumberExpString.Select(exp => integer + frac + exp)))
					.Or(NumberIntString.Then(integer => NumberFracString.Select(frac => integer + frac)))
					.Or(NumberIntString.Then(integer => NumberExpString.Select(exp => integer + exp)))
					.Or(NumberIntString)
					.Or(NumberFracString).Token();
			}
		}

		protected virtual Parser<IToken> Number
		{
			get
			{
				Debug.WriteLine($"parsing {nameof(Number)}");
				return NumberString.Select(
					x => TokenFactory.CreateToken(TokenType.LiteralNumber, x));
			}
		}

		#endregion

		#region identifier

		protected virtual Parser<char> IdentifierFirstChar
		{
			get
			{
				Debug.WriteLine($"parsing {nameof(IdentifierFirstChar)}");
				return Parse.Char(c => char.IsLetter(c) || "_$".Contains(c),
					"parsing identifier first character");
			}
		}

		protected virtual Parser<char> IdentifierTailChar
		{
			get
			{
				Debug.WriteLine($"parsing {nameof(IdentifierTailChar)}");
				return Parse.Char(c => char.IsLetterOrDigit(c) || "_$".Contains(c),
					"parsing identifier character");
			}
		}

		protected virtual Parser<string> IdentifierString
		{
			get
			{
				Debug.WriteLine($"parsing {nameof(IdentifierString)}");
				return Parse.Identifier(IdentifierFirstChar, IdentifierTailChar).Token();
			}
		}

		protected virtual Parser<IToken> Identifier
		{
			get
			{
				Debug.WriteLine($"parsing {nameof(Identifier)}");
				return IdentifierString.Select(x => TokenFactory.CreateToken(TokenType.Identifier, x));
			}
		}

		#endregion

		#region function

		/// <summary>
		/// Expr ("," Expr )*
		/// </summary>
		protected virtual Parser<IToken> FuncArgs
		{
			get
			{
				Debug.WriteLine($"parsing {nameof(FuncArgs)}");
				return Parse.ChainRightOperator(Parse.Char(',').Token(), Expr,
					(comma, e1, e2) => TokenFactory.CreateToken(TokenType.FuncArgs, e1, e2));
			}
		}

		/// <summary>
		/// <see cref="IdentifierString"/> "(" <see cref="FuncArgs"/> ")"
		/// </summary>
		protected virtual Parser<IToken> Func
		{
			get
			{
				Debug.WriteLine($"parsing {nameof(Func)}");
				return IdentifierString
					.Then(name => FuncArgs.Contained(Parse.Char('(').Token(), Parse.Char(')').Token())
						.Select(args => TokenFactory.CreateToken(TokenType.Func, name, args)));
			}
		}

		#endregion

		#region parenthesis

		/// <summary>
		/// "[" Expr "]" | "(" Expr ")" | "{" Expr "}"
		/// </summary>
		protected virtual Parser<IToken> ExprParent
		{
			get
			{
				Debug.WriteLine($"parsing {nameof(ExprParent)}");
				return ExprParent1.XOr(ExprParent2).XOr(ExprParent3).Token();
			}
		}

		/// <summary>
		/// "[" Expr "]"
		/// </summary>
		protected virtual Parser<IToken> ExprParent1
		{
			get
			{
				Debug.WriteLine($"parsing {nameof(ExprParent1)}");
				return Parse.Char('[').Token().Then(c1 => Expr.Then(e => Parse.Char(']').Token()
					.Select(c2 => TokenFactory.CreateToken(TokenType.UnaryNoOp, e))));
			}
		}

		/// <summary>
		/// "(" Expr ")"
		/// </summary>
		protected virtual Parser<IToken> ExprParent2
		{
			get
			{
				Debug.WriteLine($"parsing {nameof(ExprParent2)}");
				return Parse.Char('(').Token().Then(c1 => Expr.Then(e => Parse.Char(')').Token()
					.Select(c2 => TokenFactory.CreateToken(TokenType.UnaryNoOp, e))));
			}
		}

		/// <summary>
		/// "{" Expr "}"
		/// </summary>
		protected virtual Parser<IToken> ExprParent3
		{
			get
			{
				Debug.WriteLine($"parsing {nameof(ExprParent3)}");
				return Parse.Char('{').Token().Then(c1 => Expr.Then(e => Parse.Char('}').Token()
					.Select(c2 => TokenFactory.CreateToken(TokenType.UnaryNoOp, e))));
			}
		}

		#endregion

		#region expression

		/// <summary>
		/// <see cref="Number"/> | <see cref="Func"/> | <see cref="Identifier"/> | <see cref="ExprParent"/>
		/// </summary>
		protected virtual Parser<IToken> Atom
		{
			get
			{
				Debug.WriteLine($"parsing {nameof(Atom)}");
				return Number.XOr(Func).XOr(Identifier).XOr(ExprParent);
			}
		}

		/// <summary>
		/// ("+" | "-" | "!")? <see cref="Atom"/>
		/// </summary>
		protected virtual Parser<IToken> SignedAtom
		{
			get
			{
				Debug.WriteLine($"parsing {nameof(SignedAtom)}");
				return Parse.Chars("+-!").Token().Then(c =>
						SignedAtom.Select(sa =>
							TokenFactory.CreateToken(c == '+' ? TokenType.UnaryPlus : (c == '-' ? TokenType.UnaryMinus : TokenType.UnaryNot),
								sa)))
					.XOr(Atom);
			}
		}

		/// <summary>
		/// <see cref="SignedAtom"/> ("^" <see cref="SignedAtom"/>)*
		/// </summary>
		protected virtual Parser<IToken> ExprPow
		{
			get
			{
				Debug.WriteLine($"parsing {nameof(ExprPow)}");
				return Parse.ChainRightOperator(Parse.Char('^').Token(), SignedAtom,
					(pow, sa1, sa2) => TokenFactory.CreateToken(TokenType.BinaryPow, sa1, sa2));
			}
		}

		/// <summary>
		/// <see cref="ExprPow"/> (("*" | "/" | "\") <see cref="ExprPow"/>)*
		/// </summary>
		protected virtual Parser<IToken> ExprMulDiv
		{
			get
			{
				Debug.WriteLine($"parsing {nameof(ExprMulDiv)}");
				return Parse.ChainOperator(Parse.Chars("*/\\").Token(), ExprPow,
					(mulDiv, e1, e2) => TokenFactory.CreateToken(
						mulDiv == '*' ? TokenType.BinaryMul : (mulDiv == '/' ? TokenType.BinaryDiv : TokenType.BinaryDivInt), e1, e2));
			}
		}

		/// <summary>
		/// <see cref="ExprMulDiv"/> (("+" | "-") <see cref="ExprMulDiv"/>)*
		/// </summary>
		protected virtual Parser<IToken> ExprPlusMinus
		{
			get
			{
				Debug.WriteLine($"parsing {nameof(ExprPlusMinus)}");
				return Parse.ChainOperator(Parse.Chars("+-").Token(), ExprMulDiv,
					(plusMinus, e1, e2) => TokenFactory.CreateToken(plusMinus == '+' ? TokenType.BinaryPlus : TokenType.BinaryMinus,
						e1,
						e2));
			}
		}

		/// <summary>
		/// "&lt;" | "&gt;" | "&lt;=" | "&gt;=" | "=&lt;" | "=&gt;"
		/// </summary>
		protected virtual Parser<TokenType> RelationToken
		{
			get
			{
				Debug.WriteLine($"parsing {nameof(RelationToken)}");
				return Parse.Char('>').Then(c1 => Parse.Char('=').Optional()).Token()
					.Select(c2 => c2.IsEmpty ? TokenType.BinaryGt : TokenType.BinaryGe)
					.XOr(Parse.Char('<').Then(c1 => Parse.Char('=').Optional()).Token()
						.Select(c2 => c2.IsEmpty ? TokenType.BinaryLt : TokenType.BinaryLe))
					.XOr(Parse.Char('=').Then(c1 => Parse.Chars("<>")).Token()
						.Select(c2 => c2 == '<' ? TokenType.BinaryLe : TokenType.BinaryGe));
			}
		}

		/// <summary>
		/// <see cref="ExprPlusMinus"/> (<see cref="RelationToken"/> <see cref="ExprPlusMinus"/>)?
		/// </summary>
		protected virtual Parser<IToken> ExprRelation
		{
			get
			{
				Debug.WriteLine($"parsing {nameof(ExprRelation)}");
				return ExprPlusMinus.Then(e1 => RelationToken.Then(opt => ExprRelation
						.Select(e2 => TokenFactory.CreateToken(opt, e1, e2))))
					.Or(ExprPlusMinus);
			}
		}

		/// <summary>
		/// "==" | != | "&lt;&gt;"
		/// </summary>
		protected virtual Parser<TokenType> EqualityToken
		{
			get
			{
				Debug.WriteLine($"parsing {nameof(EqualityToken)}");
				return Parse.Char('=').Then(c1 => Parse.Char('=')).Token().Select(c2 => TokenType.BinaryEq)
					.XOr(Parse.Char('<').Then(c1 => Parse.Char('>')).Token().Select(c2 => TokenType.BinaryNe))
					.XOr(Parse.Char('!').Then(c1 => Parse.Char('=')).Token().Select(c2 => TokenType.BinaryNe));
			}
		}

		/// <summary>
		/// <see cref="ExprRelation"/> ( <see cref="EqualityToken"/> <see cref="ExprRelation"/>)?
		/// </summary>
		protected virtual Parser<IToken> ExprEquality
		{
			get
			{
				Debug.WriteLine($"parsing {nameof(ExprEquality)}");
				return ExprRelation.Then(e1 => EqualityToken.Then(opt => ExprEquality
					.Select(e2 => TokenFactory.CreateToken(opt, e1, e2)))).Or(ExprRelation);
			}
		}

		/// <summary>
		/// <see cref="ExprEquality"/> ("&amp;&amp;" <see cref="ExprEquality"/>)?
		/// </summary>
		protected virtual Parser<IToken> ExprAnd
		{
			get
			{
				Debug.WriteLine($"parsing {nameof(ExprAnd)}");
				return ExprEquality.Then(e1 => Parse.Char('&').Then(c1 => Parse.Char('&'))
						.Token()
						.Then(c2 => ExprAnd.Select(e2 => TokenFactory.CreateToken(TokenType.BinaryAnd, e1, e2))))
					.Or(ExprEquality);
			}
		}

		/// <summary>
		/// <see cref="ExprAnd"/> ("||" <see cref="ExprAnd"/>)?
		/// </summary>
		protected virtual Parser<IToken> ExprOr
		{
			get
			{
				Debug.WriteLine($"parsing {nameof(ExprOr)}");
				return ExprAnd.Then(e1 => Parse.Char('|').Then(c1 => Parse.Char('|')).Token()
					.Then(c2 => ExprOr.Select(e2 => TokenFactory.CreateToken(TokenType.BinaryOr, e1, e2)))).Or(ExprAnd);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		protected virtual Parser<IToken> ExprCond
		{
			get
			{
				Debug.WriteLine($"parsing {nameof(ExprCond)}");
				return ExprOr.Then(e1 => Parse.Char('?').Token().Then(c1 => Expr.Then(
					e2 => Parse.Char(':').Token()
						.Then(c2 => ExprCond.Select(e3 => TokenFactory.CreateToken(TokenType.TrinayCondition, e1, e2, e3)))))).Or(ExprOr);
			}
		}

		protected virtual Parser<IToken> Expr
		{
			get
			{
				Debug.WriteLine($"parsing {nameof(Expr)}");
				return ExprCond;
			}
		}

		#endregion

		public virtual IToken Tokenize(string expr)
		{
			return Expr.Parse(expr);
		}

	}
}