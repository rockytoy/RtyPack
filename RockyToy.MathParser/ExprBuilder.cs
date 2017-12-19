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
				if (args.Count != 1)
					throw new FunctionArgException("abs", 1, args.Count);
				return Math.Abs(args[0].ToDouble(c.Format));
			};

			#region datetime
			DefaultFunctions["day"] = (c, args) =>
			{
				if (args.Count != 1)
					throw new FunctionArgException("day", 1, args.Count);
				return args[0].ToDateTime(c.Format).Day;
			};

			DefaultFunctions["month"] = (c, args) =>
			{
				if (args.Count != 1)
					throw new FunctionArgException("month", 1, args.Count);
				return args[0].ToDateTime(c.Format).Month;
			};

			DefaultFunctions["year"] = (c, args) =>
			{
				if (args.Count != 1)
					throw new FunctionArgException("year", 1, args.Count);
				return args[0].ToDateTime(c.Format).Year;
			};

			DefaultFunctions["hour"] = (c, args) =>
			{
				if (args.Count != 1)
					throw new FunctionArgException("hour", 1, args.Count);
				return args[0].ToDateTime(c.Format).Hour;
			};

			DefaultFunctions["minute"] = (c, args) =>
			{
				if (args.Count != 1)
					throw new FunctionArgException("minute", 1, args.Count);
				return args[0].ToDateTime(c.Format).Minute;
			};

			DefaultFunctions["second"] = (c, args) =>
			{
				if (args.Count != 1)
					throw new FunctionArgException("second", 1, args.Count);
				return args[0].ToDateTime(c.Format).Second;
			};

			DefaultFunctions["dateserial"] = (c, args) =>
			{
				if (args.Count != 3)
					throw new FunctionArgException("dateserial", 3, args.Count);
				return new DateTime(args[0].ToInt32(c.Format), args[1].ToInt32(c.Format), args[2].ToInt32(c.Format));
			};

			DefaultFunctions["timeserial"] = (c, args) =>
			{
				if (args.Count != 3)
					throw new FunctionArgException("timeserial", 3, args.Count);
				return DateTime.MinValue.Add(new TimeSpan(args[0].ToInt32(c.Format), args[1].ToInt32(c.Format),
					args[2].ToInt32(c.Format)));
			};

			DefaultFunctions["min"] = (c, args) =>
			{
				if (args.Count < 1)
					throw new FunctionArgException("min", 1, args.Count);
				return args.Select(x => x.ToDouble(c.Format)).Min();
			};

			DefaultFunctions["max"] = (c, args) =>
			{
				if (args.Count < 1)
					throw new FunctionArgException("max", 1, args.Count);
				return args.Select(x => x.ToDouble(c.Format)).Max();
			};
			#endregion

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