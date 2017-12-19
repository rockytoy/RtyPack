using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace RockyToy.MathParser
{

	public interface IExprContextSetter<out T>
	{
		T SetVar([NotNull]string name, [NotNull]Func<IExprContext, IConvertible> val);
		T SetFunc([NotNull]string name, [NotNull]Func<IExprContext, IList<IConvertible>, IConvertible> func);
		T UnsetVar([NotNull]string name);
		T UnsetFunc([NotNull]string name);
		T ResetVar();
		T ResetFunc();
	}

	public interface IExprContext : IExprContextSetter<IExprContext>
	{
		[NotNull]
		IFormatProvider Format { get; }
		Func<IExprContext, IConvertible> GetVar([NotNull]string name);
		Func<IExprContext, IList<IConvertible>, IConvertible> GetFunc([NotNull]string name);
		bool DoubleNearZero(double val);
	}

	public class ExprContext : IExprContext
	{
		private readonly IReadOnlyDictionary<string, Func<IExprContext, IList<IConvertible>, IConvertible>> _defFunctions;
		private readonly IReadOnlyDictionary<string, Func<IExprContext, IConvertible>> _defVariables;
		private readonly IDictionary<string, Func<IExprContext, IList<IConvertible>, IConvertible>> _functions = new ConcurrentDictionary<string, Func<IExprContext, IList<IConvertible>, IConvertible>>(StringComparer.InvariantCultureIgnoreCase);
		private readonly IDictionary<string, Func<IExprContext, IConvertible>> _variables = new ConcurrentDictionary<string, Func<IExprContext, IConvertible>>(StringComparer.InvariantCultureIgnoreCase);
		public ExprContext(IFormatProvider format, IReadOnlyDictionary<string, Func<IExprContext, IList<IConvertible>, IConvertible>> functions, IReadOnlyDictionary<string, Func<IExprContext, IConvertible>> variables)
		{
			Format = format;
			_defFunctions = functions;
			_defVariables = variables;
		}

		public IFormatProvider Format { get; }
		public Func<IExprContext, IConvertible> GetVar(string name)
		{
			Func<IExprContext, IConvertible> ret;
			return _variables.TryGetValue(name, out ret) ? ret : _defVariables[name];
		}

		public Func<IExprContext, IList<IConvertible>, IConvertible> GetFunc(string name)
		{
			Func<IExprContext, IList<IConvertible>, IConvertible> ret;
			return _functions.TryGetValue(name, out ret) ? ret : _defFunctions[name];
		}

		public bool DoubleNearZero(double val)
		{
			return Math.Abs(val) < double.Epsilon * 100;
		}

		public IExprContext SetVar(string name, Func<IExprContext, IConvertible> val)
		{
			_variables[name] = val;
			return this;
		}

		public IExprContext SetFunc(string name, Func<IExprContext, IList<IConvertible>, IConvertible> func)
		{
			_functions[name] = func;
			return this;
		}

		public IExprContext UnsetVar(string name)
		{
			_variables.Remove(name);
			return this;
		}

		public IExprContext UnsetFunc(string name)
		{
			_functions.Remove(name);
			return this;
		}

		public IExprContext ResetVar()
		{
			_variables.Clear();
			return this;
		}

		public IExprContext ResetFunc()
		{
			_functions.Clear();
			return this;
		}
	}
}