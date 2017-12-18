using System;
using System.Collections.Generic;

namespace RockyToy.MathParser
{
	public interface ITokenFunction
	{
		int Id { get; }
		string Name { get; }
		IList<string> Alias { get; }
		Func<IToken, IConvertible> Func { get; }
	}

	public class TokenFunction : ITokenFunction
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public IList<string> Alias { get; set; }
		public Func<IToken, IConvertible> Func { get; set; }
	}
}