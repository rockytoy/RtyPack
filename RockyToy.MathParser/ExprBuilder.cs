using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using JetBrains.Annotations;
using RockyToy.Common;

namespace RockyToy.MathParser
{

	[PublicAPI]
	public interface IExprBuilder : IExprContextSetter<IExprBuilder>
	{
		[NotNull]
		IFormatProvider Format { get; set; }
		[NotNull]
		IDictionary<string, Func<IExprContext, IList<IConvertible>, IConvertible>> DefaultFunctions { get; }
		[NotNull]
		IDictionary<string, Func<IExprContext, IConvertible>> DefaultVariables { get; }
		IExpr Parse(string expr);
		IConvertible Eval(string expr);
	}

	public class ExprBuilder : IExprBuilder
	{
		internal ITokenizer Tokenizer { get; set; }
		public IFormatProvider Format { get; set; }
		public IDictionary<string, Func<IExprContext, IList<IConvertible>, IConvertible>> DefaultFunctions { get; } = new ConcurrentDictionary<string, Func<IExprContext, IList<IConvertible>, IConvertible>>(StringComparer.InvariantCultureIgnoreCase);
		public IDictionary<string, Func<IExprContext, IConvertible>> DefaultVariables { get; } = new ConcurrentDictionary<string, Func<IExprContext, IConvertible>>(StringComparer.InvariantCultureIgnoreCase);
		public IExpr Parse(string expr)
		{
			return new Expr(Tokenizer.Tokenize(expr), BuildContext());
		}

		public IExprContext BuildContext()
		{
			return new ExprContext(Format,
				new ReadOnlyDictionary<string, Func<IExprContext, IList<IConvertible>, IConvertible>>(DefaultFunctions),
				new ReadOnlyDictionary<string, Func<IExprContext, IConvertible>>(DefaultVariables));
		}
		
		public IConvertible Eval(string expr)
		{
			return Parse(expr).Eval();
		}

		public ExprBuilder()
		{
			Tokenizer = new Tokenizer(new TokenFactory());
			Format = CultureInfo.InvariantCulture;

			// build-in functions
			DefaultFunctions["abs"] = (c, args) =>
			{
				ArgChk.Eq(args, nameof(args), 1);
				return Math.Abs(args[0].ToDouble(c.Format));
			};

			// build-in variables
			DefaultVariables["true"] = _ => true;
			DefaultVariables["false"] = _ => false;
		}

		public IExprBuilder SetVar(string name, Func<IExprContext, IConvertible> val)
		{
			DefaultVariables[name] = val;
			return this;
		}

		public IExprBuilder SetFunc(string name, Func<IExprContext, IList<IConvertible>, IConvertible> func)
		{
			DefaultFunctions[name] = func;
			return this;
		}

		public IExprBuilder UnsetVar(string name)
		{
			DefaultVariables.Remove(name);
			return this;
		}

		public IExprBuilder UnsetFunc(string name)
		{
			DefaultFunctions.Remove(name);
			return this;
		}

		public IExprBuilder ResetVar()
		{
			DefaultVariables.Clear();
			return this;
		}

		public IExprBuilder ResetFunc()
		{
			DefaultFunctions.Clear();
			return this;
		}
	}
}