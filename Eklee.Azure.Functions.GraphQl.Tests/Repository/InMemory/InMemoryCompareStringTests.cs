using Eklee.Azure.Functions.GraphQl.Repository.InMemory;
using Shouldly;
using System;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests.Repository.InMemory
{
	[Trait(Constants.Category, Constants.UnitTests)]
	public class InMemoryCompareStringTests : InMemoryCompareTestBase
	{
		private readonly InMemoryCompareString _memoryCompareString = new InMemoryCompareString();

		[Fact]
		public void CannotHandleInt()
		{
			var inMem = new InMemItem { IntValue = 1, StrValue = "1" };

			_memoryCompareString.CanHandle(inMem, GetQueryParameter("StrValue", 1, Comparisons.Equal)).ShouldBeFalse();
		}

		[Fact]
		public void CannotHandleDate()
		{
			var inMem = new InMemItem { IntValue = 1, StrValue = "1" };

			_memoryCompareString.CanHandle(inMem, GetQueryParameter("StrValue", DateTime.Now, Comparisons.Equal)).ShouldBeFalse();
		}

		[Fact]
		public void CanHandleString()
		{
			var inMem = new InMemItem { IntValue = 1, StrValue = "1" };

			_memoryCompareString.CanHandle(inMem, GetQueryParameter("StrValue", "foobar", Comparisons.Equal)).ShouldBeTrue();
		}

		[Fact]
		public void CanCompareEqual_MeetsConditionIsTrue()
		{
			var inMem = new InMemItem { IntValue = 1, StrValue = "156" };

			_memoryCompareString.MeetsCondition(inMem, GetQueryParameter("StrValue", "156", Comparisons.Equal)).ShouldBeTrue();
		}

		[Fact]
		public void CanCompareEqual_MeetsConditionIsFalse()
		{
			var inMem = new InMemItem { IntValue = 1, StrValue = "321" };

			_memoryCompareString.MeetsCondition(inMem, GetQueryParameter("StrValue", "156", Comparisons.Equal)).ShouldBeFalse();
		}

		[Fact]
		public void CanCompareStartsWith_MeetsConditionIsTrue()
		{
			var inMem = new InMemItem { IntValue = 1, StrValue = "foo156" };

			_memoryCompareString.MeetsCondition(inMem, GetQueryParameter("StrValue", "foo", Comparisons.StringStartsWith)).ShouldBeTrue();
		}

		[Fact]
		public void CanCompareEndsWith_MeetsConditionIsTrue()
		{
			var inMem = new InMemItem { IntValue = 1, StrValue = "156bar" };

			_memoryCompareString.MeetsCondition(inMem, GetQueryParameter("StrValue", "bar", Comparisons.StringEndsWith)).ShouldBeTrue();
		}

		[Fact]
		public void CanCompareContains_MeetsConditionIsTrue()
		{
			var inMem = new InMemItem { IntValue = 1, StrValue = "foo1456bar" };

			_memoryCompareString.MeetsCondition(inMem, GetQueryParameter("StrValue", "456", Comparisons.StringContains)).ShouldBeTrue();
		}

		[Fact]
		public void CanCompareNotEquals_MeetsConditionIsTrue()
		{
			var inMem = new InMemItem { IntValue = 1, StrValue = "foo14bar" };

			_memoryCompareString.MeetsCondition(inMem, GetQueryParameter("StrValue", "foo14bar2", Comparisons.NotEqual)).ShouldBeTrue();
		}

		[Fact]
		public void CanCompareNotEquals_MeetsConditionIsFalse()
		{
			var inMem = new InMemItem { IntValue = 1, StrValue = "foo714bar" };

			_memoryCompareString.MeetsCondition(inMem, GetQueryParameter("StrValue", "foo714bar", Comparisons.NotEqual)).ShouldBeFalse();
		}
	}
}
