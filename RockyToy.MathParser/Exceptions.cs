using System;

namespace RockyToy.MathParser
{
	public class MathParserException : Exception
	{
		public MathParserException() { }
		public MathParserException(string msg) : base(msg) { }
		public MathParserException(string msg, Exception inner) : base(msg, inner) { }
	}

	public class SyntaxException : MathParserException
	{
		public SyntaxException(string expr, Exception e) : base($"Invalid syntax: {expr}", e) { }
	}

	public class UndefinedException : MathParserException
	{
		public UndefinedException() { }
		public UndefinedException(string msg) : base(msg) { }
		public UndefinedException(string msg, Exception inner) : base(msg, inner) { }
	}

	public class UndefinedVariableException : UndefinedException
	{
		public UndefinedVariableException(string name) : base($"variable {name} is undefined") { }
	}
	public class UndefinedFunctionException : UndefinedException
	{
		public UndefinedFunctionException(string name) : base($"function {name} is undefined") { }
	}

	public class FunctionArgException : MathParserException
	{
		/// <summary>
		/// invalid size
		/// </summary>
		/// <param name="funcName"></param>
		/// <param name="expectSize"></param>
		/// <param name="actualSize"></param>
		public FunctionArgException(string funcName, int expectSize, int actualSize) : base($"function {funcName} expects {expectSize} arguments but found {actualSize} arguments") { }

		/// <summary>
		/// invalid argument
		/// </summary>
		/// <param name="e"></param>
		public FunctionArgException(Exception e) : base($"failed to parse argument : {e.Message}", e) { }
		public FunctionArgException(string msg) : base(msg) { }
	}
}