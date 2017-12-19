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
		HashSet<string> FuncSet { get; }
		[NotNull]
		HashSet<string> VarSet { get; }
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
			FuncSet = new HashSet<string>(GetFunction(_root));
			VarSet = new HashSet<string>(GetVariable(_root));
		}

		internal IConvertible Arg([NotNull]IToken token, int idx)
		{
			ArgChk.NotNull(token, nameof(token));
			ArgChk.InBound(idx, nameof(idx), token.Children);
			return EvalToken(token.Children[idx]);
		}

		internal double ArgDouble( [NotNull]IToken token, int idx)
		{
			return Arg(token, idx).ToDouble(_context.Format);
		}

		internal long ArgInteger([NotNull]IToken token, int idx)
		{
			return Arg(token, idx).ToInt64(_context.Format);
		}

		internal bool ArgBool( [NotNull]IToken token, int idx)
		{
			return Arg(token, idx).ToBoolean(_context.Format);
		}

		public IConvertible Eval()
		{
			return EvalToken(_root);
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
					throw new ArgumentException("Invalid Expression (unexpected ,)");
				case TokenType.UnaryPlus:
				case TokenType.UnaryNoOp:
				{
					ArgChk.Eq(token.Children, nameof(token.Children), 1);
					return Arg(token, 0);
				}
				case TokenType.UnaryMinus:
				{
					ArgChk.Eq(token.Children, nameof(token.Children), 1);
					return -ArgDouble(token, 0);
				}
				case TokenType.UnaryNot:
				{
					ArgChk.Eq(token.Children, nameof(token.Children), 1);
					return !ArgBool(token, 0);
				}
				case TokenType.BinaryPow:
				{
					ArgChk.Eq(token.Children, nameof(token.Children), 2);
					return Math.Pow(ArgDouble(token, 0), ArgDouble(token, 1));
				}
				case TokenType.BinaryMul:
				{
					ArgChk.Eq(token.Children, nameof(token.Children), 2);
					var arg0 = ArgDouble(token, 0);
					// short circuit!!
					if (_context.DoubleNearZero(arg0))
						return 0;
					return arg0 * ArgDouble(token, 1);
				}
				case TokenType.BinaryDiv:
				{
					ArgChk.Eq(token.Children, nameof(token.Children), 2);
					return ArgDouble(token, 0) / ArgDouble(token, 1);
				}
				case TokenType.BinaryDivInt:
				{
					ArgChk.Eq(token.Children, nameof(token.Children), 2);
					var arg1 = ArgInteger(token, 1);
					var arg0 = ArgInteger(token, 0);
					if (arg1 == 0)
						return (double)arg0 / arg1;
					return ArgInteger(token, 0) / arg1;
				}
				case TokenType.BinaryPlus:
				{
					ArgChk.Eq(token.Children, nameof(token.Children), 2);
					return ArgDouble(token, 0) + ArgDouble(token, 1);
				}
				case TokenType.BinaryMinus:
				{
					ArgChk.Eq(token.Children, nameof(token.Children), 2);
					return ArgDouble(token, 0) - ArgDouble(token, 1);
				}
				case TokenType.BinaryLt:
				{
					ArgChk.Eq(token.Children, nameof(token.Children), 2);
					return ArgDouble( token, 0) < ArgDouble( token, 1);
				}
				case TokenType.BinaryLe:
				{
					ArgChk.Eq(token.Children, nameof(token.Children), 2);
					return ArgDouble( token, 0) <= ArgDouble( token, 1);
				}
				case TokenType.BinaryGt:
				{
					ArgChk.Eq(token.Children, nameof(token.Children), 2);
					return ArgDouble( token, 0) > ArgDouble( token, 1);
				}
				case TokenType.BinaryGe:
				{
					ArgChk.Eq(token.Children, nameof(token.Children), 2);
					return ArgDouble( token, 0) >= ArgDouble( token, 1);
				}
				case TokenType.BinaryEq:
				{
					ArgChk.Eq(token.Children, nameof(token.Children), 2);
					return _context.DoubleNearZero(ArgDouble( token, 0) - ArgDouble( token, 1));
				}
				case TokenType.BinaryNe:
				{
					ArgChk.Eq(token.Children, nameof(token.Children), 2);
					return !_context.DoubleNearZero(ArgDouble( token, 0) - ArgDouble( token, 1));
				}
				case TokenType.BinaryAnd:
				{
					ArgChk.Eq(token.Children, nameof(token.Children), 2);
					return ArgBool( token, 0) && ArgBool( token, 1);
				}
				case TokenType.BinaryOr:
				{
					ArgChk.Eq(token.Children, nameof(token.Children), 2);
					return ArgBool( token, 0) || ArgBool( token, 1);
				}
				case TokenType.TrinayCondition:
				{
					ArgChk.Eq(token.Children, nameof(token.Children), 3);
					return ArgBool( token, 0) ? Arg( token, 1) : Arg( token, 2);
				}
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		
		private static IEnumerable<string> GetFunction(IToken token)
		{
			if (token.Type == TokenType.Func)
				yield return token.Expr.ToLowerInvariant();
			if (token.Children == null || token.Children.Count == 0)
				yield break;
			foreach (var c in token.Children)
			foreach (var f in GetFunction(c))
				yield return f;
		}

		private static IEnumerable<string> GetVariable(IToken token)
		{
			if (token.Type == TokenType.Identifier)
				yield return token.Expr.ToLowerInvariant();
			if (token.Children == null || token.Children.Count == 0)
				yield break;
			foreach (var c in token.Children)
			foreach (var v in GetVariable(c))
				yield return v;
		}

		public HashSet<string> FuncSet { get; }
		public HashSet<string> VarSet { get; }
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