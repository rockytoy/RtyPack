using System;
using System.Collections;
using NUnit.Framework;

namespace RockyToy.MathParser.Tests
{
	public class MathParserTestcases
	{
		public static IEnumerable BasicNumber
		{
			get
			{
				yield return new TestCaseData("567").Returns(567);
				yield return new TestCaseData("-567").Returns(-567);
				yield return new TestCaseData("567.123").Returns(567.123);
				yield return new TestCaseData("-567.123").Returns(-567.123);
				yield return new TestCaseData("567e12").Returns(567e12);
				yield return new TestCaseData("-567e12").Returns(-567e12);
				yield return new TestCaseData("567.123e12").Returns(567.123e12);
				yield return new TestCaseData("-567.123e12").Returns(-567.123e12);
				yield return new TestCaseData(".123").Returns(.123);
				yield return new TestCaseData("-.123").Returns(-.123);
				yield return new TestCaseData("0.123").Returns(0.123);
				yield return new TestCaseData("-0.123").Returns(-0.123);

				yield return new TestCaseData("0").Returns(0);
				yield return new TestCaseData("-0").Returns(0);
				yield return new TestCaseData("0.0").Returns(0);
				yield return new TestCaseData("-0.0").Returns(0);
				yield return new TestCaseData(".0").Returns(0);
				yield return new TestCaseData("-.0").Returns(0);
				yield return new TestCaseData("0e10").Returns(0);
				yield return new TestCaseData("-0e10").Returns(0);
				yield return new TestCaseData("0.0e10").Returns(0);
				yield return new TestCaseData("-0.0e10").Returns(0);
			}
		}

		public static IEnumerable BasicParenthesis
		{
			get
			{
				yield return new TestCaseData("(567)").Returns(567);
				yield return new TestCaseData("(-567)").Returns(-567);
				yield return new TestCaseData("(567.123)").Returns(567.123);
				yield return new TestCaseData("(-567.123)").Returns(-567.123);
				yield return new TestCaseData("(567e12)").Returns(567e12);
				yield return new TestCaseData("(-567e12)").Returns(-567e12);
				yield return new TestCaseData("(567.123e12)").Returns(567.123e12);
				yield return new TestCaseData("(-567.123e12)").Returns(-567.123e12);
				yield return new TestCaseData("(.123)").Returns(.123);
				yield return new TestCaseData("(-.123)").Returns(-.123);
				yield return new TestCaseData("(0.123)").Returns(0.123);
				yield return new TestCaseData("(-0.123)").Returns(-0.123);

				yield return new TestCaseData("[0]").Returns(0);
				yield return new TestCaseData("[-0]").Returns(0);
				yield return new TestCaseData("[0.0]").Returns(0);
				yield return new TestCaseData("[-0.0]").Returns(0);
				yield return new TestCaseData("[.0]").Returns(0);
				yield return new TestCaseData("[-.0]").Returns(0);
				yield return new TestCaseData("{0e10}").Returns(0);
				yield return new TestCaseData("{-0e10}").Returns(0);
				yield return new TestCaseData("{0.0e10}").Returns(0);
				yield return new TestCaseData("{-0.0e10}").Returns(0);
			}
		}

		public static IEnumerable BasicArithmatic
		{
			get
			{
				yield return new TestCaseData("1+1").Returns(2);
				yield return new TestCaseData("1-1").Returns(0);
				yield return new TestCaseData("1+0").Returns(1);
				yield return new TestCaseData("1-0").Returns(1);

				yield return new TestCaseData("5*2").Returns(10);
				yield return new TestCaseData("5/2").Returns(2.5);
				yield return new TestCaseData("5\\2").Returns(2);
				yield return new TestCaseData("1*0").Returns(0);
				yield return new TestCaseData("0/1").Returns(0);
				yield return new TestCaseData("1/0").Returns(double.PositiveInfinity);
				yield return new TestCaseData("1\\0").Returns(double.PositiveInfinity);

				yield return new TestCaseData("0^0").Returns(1);
				yield return new TestCaseData("0^1").Returns(0);
				yield return new TestCaseData("1^0").Returns(1);
				yield return new TestCaseData("1^1").Returns(1);
				yield return new TestCaseData("2^10").Returns(1024);
			}
		}

		public static IEnumerable NegArithmatic
		{
			get
			{
				yield return new TestCaseData("1+-1").Returns(0);
				yield return new TestCaseData("1--1").Returns(2);
				yield return new TestCaseData("-1+0").Returns(-1);
				yield return new TestCaseData("-1-0").Returns(-1);

				yield return new TestCaseData("5*-2").Returns(-10);
				yield return new TestCaseData("5/-2").Returns(-2.5);
				yield return new TestCaseData("-5\\2").Returns(-2);
				yield return new TestCaseData("-1*0").Returns(0);
				yield return new TestCaseData("-0/1").Returns(0);
				yield return new TestCaseData("-1/0").Returns(double.NegativeInfinity);
				yield return new TestCaseData("-1\\0").Returns(double.NegativeInfinity);

				yield return new TestCaseData("0^-0").Returns(1);
				yield return new TestCaseData("-0^1").Returns(0);
				yield return new TestCaseData("-1^0").Returns(1);
				yield return new TestCaseData("1^-3").Returns(1);
				yield return new TestCaseData("-1^3").Returns(-1);
				yield return new TestCaseData("-2^9").Returns(-512);
			}
		}

		public static IEnumerable BasicFunc
		{
			get
			{
				yield return new TestCaseData("abs(-2)").Returns(2);
				yield return new TestCaseData("abs(2)").Returns(2);
			}
		}

		public static IEnumerable BasicLogic
		{
			get
			{
				yield return new TestCaseData("1 > 0").Returns(true);
				yield return new TestCaseData("0 > 1").Returns(false);
				yield return new TestCaseData("0 > 0").Returns(false);

				yield return new TestCaseData("1 < 0").Returns(false);
				yield return new TestCaseData("0 < 1").Returns(true);
				yield return new TestCaseData("0 < 0").Returns(false);

				yield return new TestCaseData("1 >= 0").Returns(true);
				yield return new TestCaseData("0 >= 1").Returns(false);
				yield return new TestCaseData("0 >= 0").Returns(true);

				yield return new TestCaseData("1 <= 0").Returns(false);
				yield return new TestCaseData("0 <= 1").Returns(true);
				yield return new TestCaseData("0 <= 0").Returns(true);

				yield return new TestCaseData("0 == 1").Returns(false);
				yield return new TestCaseData("0 == 0").Returns(true);

				yield return new TestCaseData("0 <> 1").Returns(true);
				yield return new TestCaseData("0 <> 0").Returns(false);
				yield return new TestCaseData("0 != 1").Returns(true);
				yield return new TestCaseData("0 != 0").Returns(false);
			}
		}
	}

	[TestFixture]
	public class MathParserTest
	{
		private readonly IExprBuilder _basicExprBuilder;

		public MathParserTest()
		{
			_basicExprBuilder = new ExprBuilder();
		}

		[Test,
			TestCaseSource(typeof(MathParserTestcases), nameof(MathParserTestcases.BasicNumber)),
			TestCaseSource(typeof(MathParserTestcases), nameof(MathParserTestcases.BasicParenthesis)),
			TestCaseSource(typeof(MathParserTestcases), nameof(MathParserTestcases.BasicArithmatic)),
			TestCaseSource(typeof(MathParserTestcases), nameof(MathParserTestcases.NegArithmatic)),
			TestCaseSource(typeof(MathParserTestcases), nameof(MathParserTestcases.BasicFunc)),
			TestCaseSource(typeof(MathParserTestcases), nameof(MathParserTestcases.BasicLogic))]
		public IConvertible TestBasicParser(string expr)
		{
			return _basicExprBuilder.Eval(expr);
		}
	}
}
