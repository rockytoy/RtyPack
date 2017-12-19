using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RockyToy.Common;

namespace RockyToy.MathParser
{
	[PublicAPI]
	public interface IExpr : IExprContextSetter<IExpr>
	{
		IConvertible Eval();
		[NotNull]
		HashSet<string> FuncNames { get; }
		[NotNull]
		HashSet<string> VarNames { get; }
	}

	public class Expr : IExpr
	{
		private readonly IToken _root;
		private readonly IExprContext _context;
		internal Expr([NotNull] IToken root, [NotNull] IExprContext context)
		{
			ArgChk.NotNull(root, nameof(root));
			ArgChk.NotNull(context, nameof(context));
			_root = root;
			_context = context;
			FuncNames = new HashSet<string>(GetFunctionName(_root));
			VarNames = new HashSet<string>(GetVariableName(_root));
		}

		internal void ExpectArgSize(string funcName, ICollection<IToken> expect, int actual)
		{
			if (expect.Count != actual)
				throw new FunctionArgException(funcName, expect.Count, actual);
		}

		internal IConvertible Arg([NotNull]IToken token, int idx)
		{
			ArgChk.NotNull(token, nameof(token));
			ArgChk.InBound(idx, nameof(idx), token.Children);
			return EvalToken(token.Children[idx]);
		}

		internal double ArgDouble([NotNull]IToken token, int idx)
		{
			return Arg(token, idx).ToDouble(_context.Format);
		}

		internal long ArgInteger([NotNull]IToken token, int idx)
		{
			return Arg(token, idx).ToInt64(_context.Format);
		}

		internal bool ArgBool([NotNull]IToken token, int idx)
		{
			return Arg(token, idx).ToBoolean(_context.Format);
		}

		public IConvertible Eval()
		{
			try
			{
				return EvalToken(_root);
			}
			catch (FormatException e)
			{
				throw new FunctionArgException(e);
			}
		}

		internal IConvertible EvalToken([NotNull] IToken token)
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
						return _context.GetVar(token.Expr)(_context);
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
									args.Add(EvalToken(curArg));
								else
								{
									ArgChk.NotNullOrEmpty(curArg.Children, nameof(curArg.Children));
									foreach (var c in curArg.Children)
										arg.Enqueue(c);
								}
							}
						}
						return _context.GetFunc(token.Expr)(_context, args);
					}
				case TokenType.FuncArgs:
					throw new Sprache.ParseException("unexpected comma");
				case TokenType.UnaryPlus:
				case TokenType.UnaryNoOp:
					{
						ExpectArgSize(token.Type.ToString(), token.Children, 1);
						return Arg(token, 0);
					}
				case TokenType.UnaryMinus:
					{
						ExpectArgSize(token.Type.ToString(), token.Children, 1);
						return -ArgDouble(token, 0);
					}
				case TokenType.UnaryNot:
					{
						ExpectArgSize(token.Type.ToString(), token.Children, 1);
						return !ArgBool(token, 0);
					}
				case TokenType.BinaryPow:
					{
						ExpectArgSize(token.Type.ToString(), token.Children, 2);
						return Math.Pow(ArgDouble(token, 0), ArgDouble(token, 1));
					}
				case TokenType.BinaryMul:
					{
						ExpectArgSize(token.Type.ToString(), token.Children, 2);
						var arg0 = ArgDouble(token, 0);
						// short circuit!!
						if (_context.DoubleNearZero(arg0))
							return 0;
						return arg0 * ArgDouble(token, 1);
					}
				case TokenType.BinaryDiv:
					{
						ExpectArgSize(token.Type.ToString(), token.Children, 2);
						return ArgDouble(token, 0) / ArgDouble(token, 1);
					}
				case TokenType.BinaryDivInt:
					{
						ExpectArgSize(token.Type.ToString(), token.Children, 2);
						var arg1 = ArgInteger(token, 1);
						var arg0 = ArgInteger(token, 0);
						if (arg1 == 0)
							return (double)arg0 / arg1;
						return ArgInteger(token, 0) / arg1;
					}
				case TokenType.BinaryPlus:
					{
						ExpectArgSize(token.Type.ToString(), token.Children, 2);
						return ArgDouble(token, 0) + ArgDouble(token, 1);
					}
				case TokenType.BinaryMinus:
					{
						ExpectArgSize(token.Type.ToString(), token.Children, 2);
						return ArgDouble(token, 0) - ArgDouble(token, 1);
					}
				case TokenType.BinaryLt:
					{
						ExpectArgSize(token.Type.ToString(), token.Children, 2);
						return ArgDouble(token, 0) < ArgDouble(token, 1);
					}
				case TokenType.BinaryLe:
					{
						ExpectArgSize(token.Type.ToString(), token.Children, 2);
						return ArgDouble(token, 0) <= ArgDouble(token, 1);
					}
				case TokenType.BinaryGt:
					{
						ExpectArgSize(token.Type.ToString(), token.Children, 2);
						return ArgDouble(token, 0) > ArgDouble(token, 1);
					}
				case TokenType.BinaryGe:
					{
						ExpectArgSize(token.Type.ToString(), token.Children, 2);
						return ArgDouble(token, 0) >= ArgDouble(token, 1);
					}
				case TokenType.BinaryEq:
					{
						ExpectArgSize(token.Type.ToString(), token.Children, 2);
						return _context.DoubleNearZero(ArgDouble(token, 0) - ArgDouble(token, 1));
					}
				case TokenType.BinaryNe:
					{
						ExpectArgSize(token.Type.ToString(), token.Children, 2);
						return !_context.DoubleNearZero(ArgDouble(token, 0) - ArgDouble(token, 1));
					}
				case TokenType.BinaryAnd:
					{
						ExpectArgSize(token.Type.ToString(), token.Children, 2);
						return ArgBool(token, 0) && ArgBool(token, 1);
					}
				case TokenType.BinaryOr:
					{
						ExpectArgSize(token.Type.ToString(), token.Children, 2);
						return ArgBool(token, 0) || ArgBool(token, 1);
					}
				case TokenType.TrinayCondition:
					{
						ExpectArgSize(token.Type.ToString(), token.Children, 3);
						return ArgBool(token, 0) ? Arg(token, 1) : Arg(token, 2);
					}
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private static IEnumerable<string> GetFunctionName(IToken token)
		{
			if (token.Type == TokenType.Func)
				yield return token.Expr.ToLowerInvariant();
			if (token.Children == null || token.Children.Count == 0)
				yield break;
			foreach (var c in token.Children)
				foreach (var f in GetFunctionName(c))
					yield return f;
		}

		private static IEnumerable<string> GetVariableName(IToken token)
		{
			if (token.Type == TokenType.Identifier)
				yield return token.Expr.ToLowerInvariant();
			if (token.Children == null || token.Children.Count == 0)
				yield break;
			foreach (var c in token.Children)
				foreach (var v in GetVariableName(c))
					yield return v;
		}

		public HashSet<string> FuncNames { get; }
		public HashSet<string> VarNames { get; }
		public IExpr SetVar(string name, Func<IExprContext, IConvertible> val)
		{
			_context.SetVar(name, val);
			return this;
		}

		public IExpr SetFunc(string name, Func<IExprContext, IList<IConvertible>, IConvertible> func)
		{
			_context.SetFunc(name, func);
			return this;
		}

		public IExpr UnsetVar(string name)
		{
			_context.UnsetVar(name);
			return this;
		}

		public IExpr UnsetFunc(string name)
		{
			_context.UnsetFunc(name);
			return this;
		}

		public IExpr ResetVar()
		{
			_context.ResetVar();
			return this;
		}

		public IExpr ResetFunc()
		{
			_context.ResetFunc();
			return this;
		}
	}
}