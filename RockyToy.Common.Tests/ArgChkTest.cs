using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;

namespace RockyToy.Common.Tests
{
	[TestFixture]
	public class ArgChkTest
	{
		private readonly object _nullObj = null;
		private readonly int? _nullInt = new int?();
		private readonly string _nullString = null;
		private readonly Func<bool> _nullPredicate = null;
		private readonly ICollection<int> _colNull = null;
		private const string ArgName = "test";
		private readonly ICollection<int> _col0 = new Collection<int>();
		private readonly ICollection<int> _col1 = new Collection<int> { 0 };
		private readonly ICollection<int> _col3 = new Collection<int> { 1, 2, 3 };
		[Test]
		public void TestNotNull()
		{
			Assert.DoesNotThrow(() => ArgChk.NotNull(0, ArgName));
			Assert.Throws<ArgumentNullException>(() => ArgChk.NotNull(_nullObj, ArgName));
			Assert.Throws<ArgumentNullException>(() => ArgChk.NotNull(_nullInt, ArgName));
		}

		[Test]
		public void TestNull()
		{
			Assert.Throws<ArgumentNullException>(() => ArgChk.Null(0, ArgName));
			Assert.DoesNotThrow(() => ArgChk.Null(_nullObj, ArgName));
			Assert.DoesNotThrow(() => ArgChk.Null(_nullInt, ArgName));
		}

		[Test]
		public void TestNotNullOrEmpty()
		{
			// string
			Assert.Throws<ArgumentNullException>(() => ArgChk.NotNullOrEmpty(_nullString, ArgName));
			Assert.Throws<ArgumentNullException>(() => ArgChk.NotNullOrEmpty("", ArgName));
			Assert.DoesNotThrow(() => ArgChk.NotNullOrEmpty("ok", ArgName));
			// ICollection
			Assert.Throws<ArgumentNullException>(() => ArgChk.NotNullOrEmpty(_colNull, ArgName));
			Assert.Throws<ArgumentNullException>(() => ArgChk.NotNullOrEmpty(_col0, ArgName));
			Assert.DoesNotThrow(() => ArgChk.NotNullOrEmpty(_col1, ArgName));
		}

		[Test]
		public void TestNullOrEmpty()
		{
			// string
			Assert.DoesNotThrow(() => ArgChk.NullOrEmpty(_nullString, ArgName));
			Assert.DoesNotThrow(() => ArgChk.NullOrEmpty("", ArgName));
			Assert.Throws<ArgumentNullException>(() => ArgChk.NullOrEmpty("ok", ArgName));
			// ICollection
			Assert.DoesNotThrow(() => ArgChk.NullOrEmpty(_colNull, ArgName));
			Assert.DoesNotThrow(() => ArgChk.NullOrEmpty(_col0, ArgName));
			Assert.Throws<ArgumentNullException>(() => ArgChk.NullOrEmpty(_col1, ArgName));
		}

		[Test]
		public void TestInBound()
		{
			// int
			//[6,5,4)
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.InBound(5, ArgName, 4, 6));
			//[5,6,4)
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.InBound(6, ArgName, 4, 5));
			//[4,6,5)
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.InBound(6, ArgName, 5, 4));
			//[6,4,5)
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.InBound(4, ArgName, 5, 6));
			//[4,5,6)
			Assert.DoesNotThrow(() => ArgChk.InBound(5, ArgName, 6, 4));
			//[5,4,6)
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.InBound(4, ArgName, 6, 5));
			//[0,-1,1)
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.InBound(-1, ArgName, 1));
			//[0,1,-1)
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.InBound(1, ArgName, -1));
			//[0,0,0)
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.InBound(0, ArgName, 0));
			//[0,0,0)
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.InBound(0, ArgName, 0));
			//[0,1,2)
			Assert.DoesNotThrow(() => ArgChk.InBound(1, ArgName, 2));
			//[0,1,1)
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.InBound(1, ArgName, 1));

			// collection
			var col3 = new Collection<int> { 1, 2, 3 };
			Assert.DoesNotThrow(() => ArgChk.InBound(0, ArgName, col3));
			Assert.DoesNotThrow(() => ArgChk.InBound(2, ArgName, col3));
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.InBound(-1, ArgName, col3));
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.InBound(3, ArgName, col3));
		}

		[Test]
		public void TestPredicate()
		{
			Assert.Throws<ArgumentNullException>(() => ArgChk.Predicate(_nullPredicate, ArgName));
			Assert.Throws<ArgumentException>(() => ArgChk.Predicate(() => false, ArgName));
			Assert.DoesNotThrow(() => ArgChk.Predicate(() => true, ArgName));
		}

		[Test]
		public void TestGt()
		{
			// int
			Assert.DoesNotThrow(() => ArgChk.Gt(1, ArgName, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.Gt(0, ArgName, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.Gt(0, ArgName, 1));
			// collection
			Assert.Throws<ArgumentNullException>(() => ArgChk.Gt(_colNull, ArgName, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.Gt(_col0, ArgName, 0));
			Assert.DoesNotThrow(() => ArgChk.Gt(_col1, ArgName, 0));
			Assert.DoesNotThrow(() => ArgChk.Gt(_col3, ArgName, 2));
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.Gt(_col3, ArgName, 4));

		}

		[Test]
		public void TestLt()
		{
			// int
			Assert.DoesNotThrow(() => ArgChk.Lt(0, ArgName, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.Lt(0, ArgName, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.Lt(1, ArgName, 0));
			// collection
			Assert.Throws<ArgumentNullException>(() => ArgChk.Lt(_colNull, ArgName, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.Lt(_col0, ArgName, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.Lt(_col1, ArgName, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.Lt(_col3, ArgName, 2));
			Assert.DoesNotThrow(() => ArgChk.Lt(_col3, ArgName, 4));
		}

		[Test]
		public void TestEq()
		{
			// int
			Assert.DoesNotThrow(() => ArgChk.Eq(0, ArgName, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.Eq(0, ArgName, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.Eq(1, ArgName, 0));
			// string
			Assert.DoesNotThrow(() => ArgChk.Eq(_nullString, ArgName, _nullString));
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.Eq(_nullString, ArgName, string.Empty));
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.Eq(string.Empty, ArgName, _nullString));
			Assert.DoesNotThrow(() => ArgChk.Eq(string.Empty, ArgName, string.Empty));
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.Eq("abc", ArgName, string.Empty));
			Assert.DoesNotThrow(() => ArgChk.Eq("abc", ArgName, "abc"));
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.Eq("xyz", ArgName, "abc"));
			// collection
			Assert.Throws<ArgumentNullException>(() => ArgChk.Eq(_colNull, ArgName, 1));
			Assert.DoesNotThrow(() => ArgChk.Eq(_col0, ArgName, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.Eq(_col1, ArgName, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.Eq(_col3, ArgName, 2));
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.Eq(_col3, ArgName, 4));
		}

		[Test]
		public void TestNe()
		{
			// int
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.Ne(0, ArgName, 0));
			Assert.DoesNotThrow(() => ArgChk.Ne(0, ArgName, 1));
			Assert.DoesNotThrow(() => ArgChk.Ne(1, ArgName, 0));
			// string
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.Ne(_nullString, ArgName, _nullString));
			Assert.DoesNotThrow(() => ArgChk.Ne(_nullString, ArgName, string.Empty));
			Assert.DoesNotThrow(() => ArgChk.Ne(string.Empty, ArgName, _nullString));
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.Ne(string.Empty, ArgName, string.Empty));
			Assert.DoesNotThrow(() => ArgChk.Ne("abc", ArgName, string.Empty));
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.Ne("abc", ArgName, "abc"));
			Assert.DoesNotThrow(() => ArgChk.Ne("xyz", ArgName, "abc"));
			// collection
			Assert.Throws<ArgumentNullException>(() => ArgChk.Ne(_colNull, ArgName, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => ArgChk.Ne(_col0, ArgName, 0));
			Assert.DoesNotThrow(() => ArgChk.Ne(_col1, ArgName, 0));
			Assert.DoesNotThrow(() => ArgChk.Ne(_col3, ArgName, 2));
			Assert.DoesNotThrow(() => ArgChk.Ne(_col3, ArgName, 4));
		}

		private enum TestEnum
		{
			T1,
			T2,
			T3,
		}

		[Test]
		public void TestDefinedInEnum()
		{
			Assert.DoesNotThrow(() => ArgChk.DefinedInEnum(TestEnum.T1, ArgName));
			Assert.Throws<ArgumentException>(() => ArgChk.DefinedInEnum((TestEnum)(-1), ArgName));
			Assert.DoesNotThrow(() => ArgChk.DefinedInEnum((TestEnum)0, ArgName));
			Assert.DoesNotThrow(() => ArgChk.DefinedInEnum((TestEnum)1, ArgName));
			Assert.DoesNotThrow(() => ArgChk.DefinedInEnum((TestEnum)2, ArgName));
			Assert.Throws<ArgumentException>(() => ArgChk.DefinedInEnum((TestEnum)4, ArgName));
		}
	}
}
