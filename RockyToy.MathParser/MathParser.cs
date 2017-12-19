using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using RockyToy.Common;

namespace RockyToy.MathParser
{
	[PublicAPI]
	public interface IExprBuilder
	{
		[NotNull]
		ITokenizer Tokenizer { get; set; }
		[NotNull]
		IFormatProvider Format { get; set; }
		[NotNull]
		IDictionary<string, Func<IExprContext, IList<IConvertible>, IConvertible>> Functions { get; }
		[NotNull]
		IDictionary<string, Func<IExprContext, IConvertible>> Variables { get; }
		IExpr Parse(string expr);
		IExprContext BuildContext();
		IExprContext BuildContext([NotNull]IReadOnlyDictionary<string, Func<IExprContext, IConvertible>> variables);
		IExprContext BuildContext([NotNull]IReadOnlyDictionary<string, Func<IExprContext, IList<IConvertible>, IConvertible>> functions, [NotNull]IReadOnlyDictionary<string, Func<IExprContext, IConvertible>> variables);

		IConvertible Eval(string expr);

	}

	public interface IExprContext
	{
		[NotNull]
		IFormatProvider Format { get; }
		[NotNull]
		IReadOnlyDictionary<string, Func<IExprContext, IList<IConvertible>, IConvertible>> Functions { get; }
		[NotNull]
		IReadOnlyDictionary<string, Func<IExprContext, IConvertible>> Variables { get; }

		bool DoubleNearZero(double val);
	}

	public class ExprContext : IExprContext
	{
		public ExprContext(IFormatProvider format, IReadOnlyDictionary<string, Func<IExprContext, IList<IConvertible>, IConvertible>> functions, IReadOnlyDictionary<string, Func<IExprContext, IConvertible>> variables)
		{
			Format = format;
			Functions = functions;
			Variables = variables;
		}

		public IFormatProvider Format { get; }
		public IReadOnlyDictionary<string, Func<IExprContext, IList<IConvertible>, IConvertible>> Functions { get; }
		public IReadOnlyDictionary<string, Func<IExprContext, IConvertible>> Variables { get; }

		public bool DoubleNearZero(double val)
		{
			return Math.Abs(val) < double.Epsilon * 100;
		}
	}

	public class ExprBuilder : IExprBuilder
	{
		public ITokenizer Tokenizer { get; set; }
		public IFormatProvider Format { get; set; }
		public IDictionary<string, Func<IExprContext, IList<IConvertible>, IConvertible>> Functions { get; } = new ConcurrentDictionary<string, Func<IExprContext, IList<IConvertible>, IConvertible>>(StringComparer.OrdinalIgnoreCase);
		public IDictionary<string, Func<IExprContext, IConvertible>> Variables { get; } = new ConcurrentDictionary<string, Func<IExprContext, IConvertible>>(StringComparer.OrdinalIgnoreCase);
		public IExpr Parse(string expr)
		{
			return new Expr(Tokenizer.Tokenize(expr));
		}

		public IExprContext BuildContext()
		{
			return new ExprContext(Format,
				new ReadOnlyDictionary<string, Func<IExprContext, IList<IConvertible>, IConvertible>>(Functions),
				new ReadOnlyDictionary<string, Func<IExprContext, IConvertible>>(Variables));
		}

		public IExprContext BuildContext(IReadOnlyDictionary<string, Func<IExprContext, IConvertible>> variables)
		{
			ArgChk.NotNull(variables, nameof(variables));
			var mergedVar = new ConcurrentDictionary<string, Func<IExprContext, IConvertible>>(Variables, StringComparer.OrdinalIgnoreCase);
			foreach (var v in variables)
				mergedVar[v.Key] = v.Value;
			return new ExprContext(Format,
				new ReadOnlyDictionary<string, Func<IExprContext, IList<IConvertible>, IConvertible>>(Functions),
				new ReadOnlyDictionary<string, Func<IExprContext, IConvertible>>(mergedVar));
		}

		public IExprContext BuildContext(IReadOnlyDictionary<string, Func<IExprContext, IList<IConvertible>, IConvertible>> functions, IReadOnlyDictionary<string, Func<IExprContext, IConvertible>> variables)
		{
			ArgChk.NotNull(functions, nameof(functions));
			ArgChk.NotNull(variables, nameof(variables));
			var mergedVar = new ConcurrentDictionary<string, Func<IExprContext, IConvertible>>(Variables, StringComparer.OrdinalIgnoreCase);
			foreach (var v in variables)
				mergedVar[v.Key] = v.Value;
			var mergedFunc = new ConcurrentDictionary<string, Func<IExprContext, IList<IConvertible>, IConvertible>>(Functions, StringComparer.OrdinalIgnoreCase);
			foreach (var f in functions)
				mergedFunc[f.Key] = f.Value;
			return new ExprContext(Format,
				new ReadOnlyDictionary<string, Func<IExprContext, IList<IConvertible>, IConvertible>>(mergedFunc),
				new ReadOnlyDictionary<string, Func<IExprContext, IConvertible>>(mergedVar));
		}

		public IConvertible Eval(string expr)
		{
			return Parse(expr).Eval(BuildContext());
		}

		public ExprBuilder()
		{
			Tokenizer = new Tokenizer(new TokenFactory());
			Format = CultureInfo.InvariantCulture;

			// build-in functions
			Functions["abs"] = (c, args) =>
			{
				ArgChk.Eq(args, nameof(args), 1);
				return Math.Abs(args[0].ToDouble(c.Format));
			};

			// build-in variables
			Variables["true"] = _ => true;
			Variables["false"] = _ => false;
		}
	}
	
	[PublicAPI]
	public interface IExpr
	{
		IConvertible Eval([NotNull]IExprContext context);
		IToken Root { get; }
		IEnumerable<string> Functions { get; }
		IEnumerable<string> Variables { get; }
	}

	public class Expr : IExpr
	{
		internal Expr([NotNull] IToken root)
		{
			ArgChk.NotNull(root, nameof(root));
			Root = root;
		}

		protected IConvertible Arg([NotNull]IExprContext context, [NotNull]IToken token, int idx)
		{
			ArgChk.NotNull(token, nameof(token));
			ArgChk.InBound(idx, nameof(idx), token.Children);
			return EvalToken(context, token.Children[idx]);
		}

		protected double ArgDouble([NotNull]IExprContext context, [NotNull]IToken token, int idx)
		{
			return Arg(context, token, idx).ToDouble(context.Format);
		}

		protected long ArgInteger([NotNull]IExprContext context, [NotNull]IToken token, int idx)
		{
			return Arg(context, token, idx).ToInt64(context.Format);
		}

		protected bool ArgBool([NotNull]IExprContext context, [NotNull]IToken token, int idx)
		{
			return Arg(context, token, idx).ToBoolean(context.Format);
		}

		public IConvertible Eval(IExprContext context)
		{
			return EvalToken(context, Root);
		}

		protected IConvertible EvalToken([NotNull] IExprContext context, [NotNull] IToken token)
		{
			ArgChk.NotNull(token, nameof(token));

			switch (token.Type)
			{
				case TokenType.LiteralNumber:
					{
						ArgChk.NullOrEmpty(token.Children, nameof(token.Children));
						return double.Parse(token.Expr);
					}
				case TokenType.Identifier:
					{
						ArgChk.NullOrEmpty(token.Children, nameof(token.Children));
						return context.Variables[token.Expr](context);
					}
				case TokenType.Func:
					{
						// parse child argument
						var args = new List<IConvertible>();
						if (token.Children?.Any() ?? false)
						{
							var arg = new Queue<IToken>(token.Children);
							while (arg.Count > 0)
							{
								var curArg = arg.Dequeue();
								if (curArg.Type != TokenType.FuncArgs)
									args.Add(EvalToken(context, curArg));
								else
								{
									ArgChk.NotNullOrEmpty(curArg.Children, nameof(curArg.Children));
									foreach (var c in curArg.Children)
										arg.Enqueue(c);
								}
							}
						}
						return context.Functions[token.Expr](context, args);
					}
				case TokenType.FuncArgs:
					throw new ArgumentException("Invalid Expression (unexpected ,)");
				case TokenType.UnaryPlus:
				case TokenType.UnaryNoOp:
					{
						ArgChk.Eq(token.Children, nameof(token.Children), 1);
						return Arg(context, token, 0);
					}
				case TokenType.UnaryMinus:
					{
						ArgChk.Eq(token.Children, nameof(token.Children), 1);
						return -ArgDouble(context, token, 0);
					}
				case TokenType.UnaryNot:
					{
						ArgChk.Eq(token.Children, nameof(token.Children), 1);
						return !ArgBool(context, token, 0);
					}
				case TokenType.BinaryPow:
					{
						ArgChk.Eq(token.Children, nameof(token.Children), 2);
						return Math.Pow(ArgDouble(context, token, 0), ArgDouble(context, token, 1));
					}
				case TokenType.BinaryMul:
					{
						ArgChk.Eq(token.Children, nameof(token.Children), 2);
						var arg0 = ArgDouble(context, token, 0);
						// short circuit!!
						if (context.DoubleNearZero(arg0))
							return 0;
						return arg0 * ArgDouble(context, token, 1);
					}
				case TokenType.BinaryDiv:
					{
						ArgChk.Eq(token.Children, nameof(token.Children), 2);
						return ArgDouble(context, token, 0) / ArgDouble(context, token, 1);
					}
				case TokenType.BinaryDivInt:
					{
						ArgChk.Eq(token.Children, nameof(token.Children), 2);
						var arg1 = ArgInteger(context, token, 1);
						var arg0 = ArgInteger(context, token, 0);
						if (arg1 == 0)
							return (double)arg0 / arg1;
						return ArgInteger(context, token, 0) / arg1;
					}
				case TokenType.BinaryPlus:
					{
						ArgChk.Eq(token.Children, nameof(token.Children), 2);
						return ArgDouble(context, token, 0) + ArgDouble(context, token, 1);
					}
				case TokenType.BinaryMinus:
					{
						ArgChk.Eq(token.Children, nameof(token.Children), 2);
						return ArgDouble(context, token, 0) - ArgDouble(context, token, 1);
					}
				case TokenType.BinaryLt:
					{
						ArgChk.Eq(token.Children, nameof(token.Children), 2);
						return ArgDouble(context, token, 0) < ArgDouble(context, token, 1);
					}
				case TokenType.BinaryLe:
					{
						ArgChk.Eq(token.Children, nameof(token.Children), 2);
						return ArgDouble(context, token, 0) <= ArgDouble(context, token, 1);
					}
				case TokenType.BinaryGt:
					{
						ArgChk.Eq(token.Children, nameof(token.Children), 2);
						return ArgDouble(context, token, 0) > ArgDouble(context, token, 1);
					}
				case TokenType.BinaryGe:
					{
						ArgChk.Eq(token.Children, nameof(token.Children), 2);
						return ArgDouble(context, token, 0) >= ArgDouble(context, token, 1);
					}
				case TokenType.BinaryEq:
					{
						ArgChk.Eq(token.Children, nameof(token.Children), 2);
						return context.DoubleNearZero(ArgDouble(context, token, 0) - ArgDouble(context, token, 1));
					}
				case TokenType.BinaryNe:
					{
						ArgChk.Eq(token.Children, nameof(token.Children), 2);
						return !context.DoubleNearZero(ArgDouble(context, token, 0) - ArgDouble(context, token, 1));
					}
				case TokenType.BinaryAnd:
					{
						ArgChk.Eq(token.Children, nameof(token.Children), 2);
						return ArgBool(context, token, 0) && ArgBool(context, token, 1);
					}
				case TokenType.BinaryOr:
					{
						ArgChk.Eq(token.Children, nameof(token.Children), 2);
						return ArgBool(context, token, 0) || ArgBool(context, token, 1);
					}
				case TokenType.TrinayCondition:
					{
						ArgChk.Eq(token.Children, nameof(token.Children), 3);
						return ArgBool(context, token, 0) ? Arg(context, token, 1) : Arg(context, token, 2);
					}
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public IToken Root { get; }

		private static IEnumerable<string> GetFunction(IToken token)
		{
			if (token.Type == TokenType.Func)
				yield return token.Expr;
			if (token.Children == null || token.Children.Count == 0)
				yield break;
			foreach (var c in token.Children)
				foreach (var f in GetFunction(c))
					yield return f;
		}

		private static IEnumerable<string> GetVariable(IToken token)
		{
			if (token.Type == TokenType.Identifier)
				yield return token.Expr;
			if (token.Children == null || token.Children.Count == 0)
				yield break;
			foreach (var c in token.Children)
				foreach (var v in GetVariable(c))
					yield return v;
		}

		public IEnumerable<string> Functions => GetFunction(Root);
		public IEnumerable<string> Variables => GetVariable(Root);
	}
}
