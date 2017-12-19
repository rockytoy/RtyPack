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
				yield return new TestCaseData("dateserial(2000,10,27)").Returns(new DateTime(2000,10,27));
				yield return new TestCaseData("timeserial(12,34,56)").Returns(DateTime.MinValue.Add(new TimeSpan(12,34,56)));
				yield return new TestCaseData("min(2000,10,27)").Returns(10);
				yield return new TestCaseData("max(56,12,34)").Returns(56);
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

				yield return new TestCaseData("0 > 1?10:20").Returns(20);
				yield return new TestCaseData("0 < 1?10:20").Returns(10);
				yield return new TestCaseData("true?false:true").Returns(false);
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

		[Test]
		public void TestException()
		{
			Assert.Throws<SyntaxException>(() => TestBasicParser(""));
			Assert.Throws<SyntaxException>(() => TestBasicParser("+"));
			Assert.Throws<SyntaxException>(() => TestBasicParser("{()"));
			// number
			Assert.Throws<SyntaxException>(() => TestBasicParser("12."));
			Assert.Throws<SyntaxException>(() => TestBasicParser("12.."));
			Assert.Throws<SyntaxException>(() => TestBasicParser("12..32"));
			Assert.Throws<SyntaxException>(() => TestBasicParser("12.32e13e15"));
			Assert.Throws<SyntaxException>(() => TestBasicParser("12.e15"));
			Assert.Throws<SyntaxException>(() => TestBasicParser("12.0e15.15"));
			Assert.Throws<SyntaxException>(() => TestBasicParser("12e5.15"));
			Assert.Throws<SyntaxException>(() => TestBasicParser("12e5..15"));
			Assert.Throws<SyntaxException>(() => TestBasicParser("12e5..15e51"));
			Assert.Throws<SyntaxException>(() => TestBasicParser("12ee51"));
			// basic
			Assert.Throws<SyntaxException>(() => TestBasicParser("1+"));
			Assert.Throws<SyntaxException>(() => TestBasicParser("1.23/"));

			// random string
			Assert.Throws<SyntaxException>(() => TestBasicParser("#(Y"));
			Assert.Throws<SyntaxException>(() => TestBasicParser("^%#)"));
			Assert.Throws<SyntaxException>(() => TestBasicParser("(Y%)blf"));

			// undefined
			Assert.Throws<UndefinedVariableException>(() => TestBasicParser("x"));
			Assert.Throws<UndefinedVariableException>(() => TestBasicParser("xyz"));
			Assert.Throws<UndefinedVariableException>(() => TestBasicParser("x*y"));
			Assert.Throws<UndefinedVariableException>(() => TestBasicParser("z +0"));
			Assert.Throws<UndefinedFunctionException>(() => TestBasicParser("_()"));
			Assert.Throws<UndefinedFunctionException>(() => TestBasicParser("undefined(0)"));
			Assert.Throws<UndefinedFunctionException>(() => TestBasicParser("552+undefined(0)"));
			Assert.Throws<UndefinedFunctionException>(() => TestBasicParser("undefined(0)^3"));
			Assert.Catch<UndefinedException>(() => TestBasicParser("undefined(x)"));
			Assert.Catch<UndefinedException>(() => TestBasicParser("x+undefined(0)"));
			Assert.Catch<UndefinedException>(() => TestBasicParser("undefined(0)&&z"));
			Assert.Catch<UndefinedException>(() => TestBasicParser("undefined(z)?abc:xyz"));

		}
	}
}
