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
	public interface IParserBuilder
	{
		[NotNull]
		ITokenizer Tokenizer { get; set; }
		[NotNull]
		IFormatProvider Format { get; set; }
		[NotNull]
		IDictionary<string, Func<IParserContext, IList<IConvertible>, IConvertible>> Functions { get; }
		[NotNull]
		IDictionary<string, Func<IParserContext, IConvertible>> Variables { get; }
		IParser GetParser();
		IParser GetParser(IParserContext context);
		IParser GetParser([NotNull]IReadOnlyDictionary<string, Func<IParserContext, IConvertible>> variables);
		IParser GetParser([NotNull]IReadOnlyDictionary<string, Func<IParserContext, IList<IConvertible>, IConvertible>> functions, [NotNull]IReadOnlyDictionary<string, Func<IParserContext, IConvertible>> variables);
	}

	public interface IParserContext
	{
		[NotNull]
		ITokenizer Tokenizer { get; }
		[NotNull]
		IFormatProvider Format { get; }
		[NotNull]
		IReadOnlyDictionary<string, Func<IParserContext, IList<IConvertible>, IConvertible>> Functions { get; }
		[NotNull]
		IReadOnlyDictionary<string, Func<IParserContext, IConvertible>> Variables { get; }
		
		bool DoubleNearZero(double val);
	}

	public class ParserContext : IParserContext
	{
		public ParserContext(ITokenizer tokenizer, IFormatProvider format, IReadOnlyDictionary<string, Func<IParserContext, IList<IConvertible>, IConvertible>> functions, IReadOnlyDictionary<string, Func<IParserContext, IConvertible>> variables)
		{
			Tokenizer = tokenizer;
			Format = format;
			Functions = functions;
			Variables = variables;
		}

		public ITokenizer Tokenizer { get; }
		public IFormatProvider Format { get; }
		public IReadOnlyDictionary<string, Func<IParserContext, IList<IConvertible>, IConvertible>> Functions { get; }
		public IReadOnlyDictionary<string, Func<IParserContext, IConvertible>> Variables { get; }
		
		public bool DoubleNearZero(double val)
		{
			return Math.Abs(val) < double.Epsilon * 100;
		}
	}

	public class ParserBuilder : IParserBuilder
	{
		public ITokenizer Tokenizer { get; set; }
		public IFormatProvider Format { get; set; }
		public IDictionary<string, Func<IParserContext, IList<IConvertible>, IConvertible>> Functions { get; } = new ConcurrentDictionary<string, Func<IParserContext, IList<IConvertible>, IConvertible>>(StringComparer.OrdinalIgnoreCase);
		public IDictionary<string, Func<IParserContext, IConvertible>> Variables { get; } = new ConcurrentDictionary<string, Func<IParserContext, IConvertible>>(StringComparer.OrdinalIgnoreCase);

		public IParser GetParser(IParserContext context)
		{
			return new Parser(context);
		}

		public IParser GetParser()
		{
			return GetParser(new ParserContext(Tokenizer, Format,
				new ReadOnlyDictionary<string, Func<IParserContext, IList<IConvertible>, IConvertible>>(Functions),
				new ReadOnlyDictionary<string, Func<IParserContext, IConvertible>>(Variables)));
		}

		public IParser GetParser(IReadOnlyDictionary<string, Func<IParserContext, IConvertible>> variables)
		{
			ArgChk.NotNull(variables, nameof(variables));
			var mergedVar = new ConcurrentDictionary<string, Func<IParserContext, IConvertible>>(Variables, StringComparer.OrdinalIgnoreCase);
			foreach (var v in variables)
				mergedVar[v.Key] = v.Value;
			return GetParser(new ParserContext(Tokenizer, Format,
				new ReadOnlyDictionary<string, Func<IParserContext, IList<IConvertible>, IConvertible>>(Functions),
				new ReadOnlyDictionary<string, Func<IParserContext, IConvertible>>(mergedVar)));
		}

		public IParser GetParser(IReadOnlyDictionary<string, Func<IParserContext, IList<IConvertible>, IConvertible>> functions, IReadOnlyDictionary<string, Func<IParserContext, IConvertible>> variables)
		{
			ArgChk.NotNull(functions, nameof(functions));
			ArgChk.NotNull(variables, nameof(variables));
			var mergedVar = new ConcurrentDictionary<string, Func<IParserContext, IConvertible>>(Variables, StringComparer.OrdinalIgnoreCase);
			foreach (var v in variables)
				mergedVar[v.Key] = v.Value;
			var mergedFunc = new ConcurrentDictionary<string, Func<IParserContext, IList<IConvertible>, IConvertible>>(Functions, StringComparer.OrdinalIgnoreCase);
			foreach (var f in functions)
				mergedFunc[f.Key] = f.Value;
			return GetParser(new ParserContext(Tokenizer, Format,
				new ReadOnlyDictionary<string, Func<IParserContext, IList<IConvertible>, IConvertible>>(mergedFunc),
				new ReadOnlyDictionary<string, Func<IParserContext, IConvertible>>(mergedVar)));
		}

		public ParserBuilder()
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

	public interface IParser
	{
		IConvertible Parse([NotNull] string expr);
	}

	public class Parser : IParser
	{
		private readonly IParserContext _context;
		public Parser(IParserContext context)
		{
			ArgChk.NotNull(context, nameof(context));
			_context = context;
		}

		protected virtual IConvertible Arg([NotNull]IToken token, int idx)
		{
			ArgChk.NotNull(token, nameof(token));
			ArgChk.InBound(idx, nameof(idx), token.Children);
			return ParseToken(token.Children[idx]);
		}

		protected virtual double ArgDouble([NotNull]IToken token, int idx)
		{
			return Arg(token, idx).ToDouble(_context.Format);
		}

		protected virtual long ArgInteger([NotNull]IToken token, int idx)
		{
			return Arg(token, idx).ToInt64(_context.Format);
		}

		protected virtual bool ArgBool([NotNull]IToken token, int idx)
		{
			return Arg(token, idx).ToBoolean(_context.Format);
		}

		public virtual IConvertible Parse(string expr)
		{
			ArgChk.NotNullOrEmpty(expr, nameof(expr));

			return ParseToken(_context.Tokenizer.Tokenize(expr));
		}

		public virtual IConvertible ParseToken([NotNull]IToken token)
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
						return _context.Variables[token.Expr](_context);
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
									args.Add(ParseToken(curArg));
								else
								{
									ArgChk.NotNullOrEmpty(curArg.Children, nameof(curArg.Children));
									foreach (var c in curArg.Children)
									{
										arg.Enqueue(c);
									}
								}
							}
						}
						return _context.Functions[token.Expr](_context, args);
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
						return ArgDouble(token, 0) < ArgDouble(token, 1);
					}
				case TokenType.BinaryLe:
					{
						ArgChk.Eq(token.Children, nameof(token.Children), 2);
						return ArgDouble(token, 0) <= ArgDouble(token, 1);
					}
				case TokenType.BinaryGt:
					{
						ArgChk.Eq(token.Children, nameof(token.Children), 2);
						return ArgDouble(token, 0) > ArgDouble(token, 1);
					}
				case TokenType.BinaryGe:
					{
						ArgChk.Eq(token.Children, nameof(token.Children), 2);
						return ArgDouble(token, 0) >= ArgDouble(token, 1);
					}
				case TokenType.BinaryEq:
					{
						ArgChk.Eq(token.Children, nameof(token.Children), 2);
						return _context.DoubleNearZero(ArgDouble(token, 0) - ArgDouble(token, 1));
					}
				case TokenType.BinaryNe:
					{
						ArgChk.Eq(token.Children, nameof(token.Children), 2);
						return !_context.DoubleNearZero(ArgDouble(token, 0) - ArgDouble(token, 1));
					}
				case TokenType.BinaryAnd:
					{
						ArgChk.Eq(token.Children, nameof(token.Children), 2);
						return ArgBool(token, 0) && ArgBool(token, 1);
					}
				case TokenType.BinaryOr:
					{
						ArgChk.Eq(token.Children, nameof(token.Children), 2);
						return ArgBool(token, 0) || ArgBool(token, 1);
					}
				case TokenType.TrinayCondition:
					{
						ArgChk.Eq(token.Children, nameof(token.Children), 3);
						return ArgBool(token, 0) ? Arg(token, 1) : Arg(token, 2);
					}
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}

}
