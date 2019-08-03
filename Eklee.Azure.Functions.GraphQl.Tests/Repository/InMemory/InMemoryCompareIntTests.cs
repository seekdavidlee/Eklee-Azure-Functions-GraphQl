using Eklee.Azure.Functions.GraphQl.Repository.InMemory;
using Shouldly;
using System;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests.Repository.InMemory
{
	[Trait(Constants.Category, Constants.UnitTests)]
	public class InMemoryCompareIntTests : InMemoryCompareTestBase
	{
		private readonly InMemoryCompareInt _memoryCompareInt = new InMemoryCompareInt();

		[Fact]
		public void CannotHandleString()
		{
			var inMem = new InMemItem { IntValue = 1 };

			_memoryCompareInt.CanHandle(inMem, GetQueryParameter("IntValue", "1", Comparisons.Equal)).ShouldBeFalse();
		}

		[Fact]
		public void CannotHandleDate()
		{
			var inMem = new InMemItem { IntValue = 1 };

			_memoryCompareInt.CanHandle(inMem, GetQueryParameter("IntValue", DateTime.Now, Comparisons.Equal)).ShouldBeFalse();
		}

		[Fact]
		public void CanHandleInt()
		{
			var inMem = new InMemItem { IntValue = 145 };

			_memoryCompareInt.CanHandle(inMem, GetQueryParameter("IntValue", 145, Comparisons.Equal)).ShouldBeTrue();
		}

		[Fact]
		public void CanCompareEqual_MeetsConditionIsTrue()
		{
			var inMem = new InMemItem { IntValue = 111 };

			_memoryCompareInt.MeetsCondition(inMem, GetQueryParameter("IntValue", 111, Comparisons.Equal)).ShouldBeTrue();
		}

		[Fact]
		public void CanCompareEqual_MeetsConditionIsFalse()
		{
			var inMem = new InMemItem { IntValue = 321, StrValue = "321" };

			_memoryCompareInt.MeetsCondition(inMem, GetQueryParameter("IntValue", 123, Comparisons.Equal)).ShouldBeFalse();
		}

		[Fact]
		public void CanCompareNotEquals_MeetsConditionIsFalse()
		{
			var inMem = new InMemItem { IntValue = 321, StrValue = "321" };

			_memoryCompareInt.MeetsCondition(inMem, GetQueryParameter("IntValue", 321, Comparisons.NotEqual)).ShouldBeFalse();
		}

		[Fact]
		public void CanCompareNotEquals_MeetsConditionIsTrue()
		{
			var inMem = new InMemItem { IntValue = 6321, StrValue = "321" };

			_memoryCompareInt.MeetsCondition(inMem, GetQueryParameter("IntValue", 5321, Comparisons.NotEqual)).ShouldBeTrue();
		}

		[Fact]
		public void CanCompareGreaterThan_MeetsConditionIsTrue()
		{
			var inMem = new InMemItem { IntValue = 125 };

			_memoryCompareInt.MeetsCondition(inMem, GetQueryParameter("IntValue", 124, Comparisons.GreaterThan)).ShouldBeTrue();
		}

		[Fact]
		public void CanCompareGreaterThan_MeetsConditionIsFalse()
		{
			var inMem = new InMemItem { IntValue = 122 };

			_memoryCompareInt.MeetsCondition(inMem, GetQueryParameter("IntValue", 123, Comparisons.GreaterThan)).ShouldBeFalse();
		}

		[Fact]
		public void CanCompareGreaterThanWithSameNumbers_MeetsConditionIsFalse()
		{
			var inMem = new InMemItem { IntValue = 123 };

			_memoryCompareInt.MeetsCondition(inMem, GetQueryParameter("IntValue", 123, Comparisons.GreaterThan)).ShouldBeFalse();
		}

		[Fact]
		public void CanCompareGreaterEqualThanWithSameNumbers_MeetsConditionIsTrue()
		{
			var inMem = new InMemItem { IntValue = 128 };

			_memoryCompareInt.MeetsCondition(inMem, GetQueryParameter("IntValue", 128, Comparisons.GreaterEqualThan)).ShouldBeTrue();
		}

		[Fact]
		public void CanCompareGreaterEqualThan_MeetsConditionIsTrue()
		{
			var inMem = new InMemItem { IntValue = 125 };

			_memoryCompareInt.MeetsCondition(inMem, GetQueryParameter("IntValue", 124, Comparisons.GreaterEqualThan)).ShouldBeTrue();
		}

		[Fact]
		public void CanCompareLessEqualThanWithSameNumbers_MeetsConditionIsTrue()
		{
			var inMem = new InMemItem { IntValue = 1284 };

			_memoryCompareInt.MeetsCondition(inMem, GetQueryParameter("IntValue", 1284, Comparisons.LessEqualThan)).ShouldBeTrue();
		}

		[Fact]
		public void CanCompareLessEqualThan_MeetsConditionIsTrue()
		{
			var inMem = new InMemItem { IntValue = 123 };

			_memoryCompareInt.MeetsCondition(inMem, GetQueryParameter("IntValue", 124, Comparisons.LessEqualThan)).ShouldBeTrue();
		}

		[Fact]
		public void CanCompareLessEqualThan_MeetsConditionIsFalse()
		{
			var inMem = new InMemItem { IntValue = 125 };

			_memoryCompareInt.MeetsCondition(inMem, GetQueryParameter("IntValue", 124, Comparisons.LessEqualThan)).ShouldBeFalse();
		}

		[Fact]
		public void CanCompareLessThan_MeetsConditionIsTrue()
		{
			var inMem = new InMemItem { IntValue = 123 };

			_memoryCompareInt.MeetsCondition(inMem, GetQueryParameter("IntValue", 124, Comparisons.LessThan)).ShouldBeTrue();
		}

		[Fact]
		public void CanCompareLessThan_MeetsConditionIsFalse()
		{
			var inMem = new InMemItem { IntValue = 125 };

			_memoryCompareInt.MeetsCondition(inMem, GetQueryParameter("IntValue", 124, Comparisons.LessThan)).ShouldBeFalse();
		}
	}
}
