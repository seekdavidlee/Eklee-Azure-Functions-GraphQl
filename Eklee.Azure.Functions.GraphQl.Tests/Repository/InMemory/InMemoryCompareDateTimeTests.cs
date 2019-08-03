using Eklee.Azure.Functions.GraphQl.Repository.InMemory;
using Shouldly;
using System;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests.Repository.InMemory
{
	[Trait(Constants.Category, Constants.UnitTests)]
	public class InMemoryCompareDateTimeTests : InMemoryCompareTestBase
	{
		private readonly InMemoryCompareDateTime _memoryCompareDateTime = new InMemoryCompareDateTime();


		[Fact]
		public void CannotHandleString()
		{
			var inMem = new InMemItem { DateTimeValue = DateTime.Now };

			_memoryCompareDateTime.CanHandle(inMem, GetQueryParameter("DateTimeValue", "1", Comparisons.Equal)).ShouldBeFalse();
		}

		[Fact]
		public void CannotHandleInt()
		{
			var inMem = new InMemItem { DateTimeValue = DateTime.Now };

			_memoryCompareDateTime.CanHandle(inMem, GetQueryParameter("DateTimeValue", 67, Comparisons.Equal)).ShouldBeFalse();
		}

		[Fact]
		public void CanHandleDate()
		{
			var inMem = new InMemItem { DateTimeValue = DateTime.Now };

			_memoryCompareDateTime.CanHandle(inMem, GetQueryParameter("DateTimeValue", DateTime.Now, Comparisons.Equal)).ShouldBeTrue();
		}

		[Fact]
		public void CanCompareEqual_MeetsConditionIsTrue()
		{
			var inMem = new InMemItem { DateTimeValue = DateTime.Today };

			_memoryCompareDateTime.MeetsCondition(inMem, GetQueryParameter("DateTimeValue", DateTime.Today, Comparisons.Equal)).ShouldBeTrue();
		}

		[Fact]
		public void CanCompareEqual_MeetsConditionIsFalse()
		{
			var inMem = new InMemItem { DateTimeValue = DateTime.Today.AddDays(1), StrValue = "321" };

			_memoryCompareDateTime.MeetsCondition(inMem, GetQueryParameter("DateTimeValue", DateTime.Today, Comparisons.Equal)).ShouldBeFalse();
		}

		[Fact]
		public void CanCompareNotEquals_MeetsConditionIsFalse()
		{
			var inMem = new InMemItem { DateTimeValue = DateTime.Today, StrValue = "321" };

			_memoryCompareDateTime.MeetsCondition(inMem, GetQueryParameter("DateTimeValue", DateTime.Today, Comparisons.NotEqual)).ShouldBeFalse();
		}

		[Fact]
		public void CanCompareNotEquals_MeetsConditionIsTrue()
		{
			var inMem = new InMemItem { DateTimeValue = DateTime.Today.AddDays(-1), StrValue = "321" };

			_memoryCompareDateTime.MeetsCondition(inMem, GetQueryParameter("DateTimeValue", DateTime.Today, Comparisons.NotEqual)).ShouldBeTrue();
		}

		[Fact]
		public void CanCompareGreaterThan_MeetsConditionIsTrue()
		{
			var inMem = new InMemItem { DateTimeValue = DateTime.Today.AddDays(1) };

			_memoryCompareDateTime.MeetsCondition(inMem, GetQueryParameter("DateTimeValue", DateTime.Today, Comparisons.GreaterThan)).ShouldBeTrue();
		}

		[Fact]
		public void CanCompareGreaterThan_MeetsConditionIsFalse()
		{
			var inMem = new InMemItem { DateTimeValue = DateTime.Today.AddDays(-1) };

			_memoryCompareDateTime.MeetsCondition(inMem, GetQueryParameter("DateTimeValue", DateTime.Today, Comparisons.GreaterThan)).ShouldBeFalse();
		}

		[Fact]
		public void CanCompareGreaterThanWithSameNumbers_MeetsConditionIsFalse()
		{
			var inMem = new InMemItem { DateTimeValue = DateTime.Today };

			_memoryCompareDateTime.MeetsCondition(inMem, GetQueryParameter("DateTimeValue", DateTime.Today, Comparisons.GreaterThan)).ShouldBeFalse();
		}

		[Fact]
		public void CanCompareGreaterEqualThanWithSameNumbers_MeetsConditionIsTrue()
		{
			var inMem = new InMemItem { DateTimeValue = DateTime.Today };

			_memoryCompareDateTime.MeetsCondition(inMem, GetQueryParameter("DateTimeValue", DateTime.Today, Comparisons.GreaterEqualThan)).ShouldBeTrue();
		}

		[Fact]
		public void CanCompareGreaterEqualThan_MeetsConditionIsTrue()
		{
			var inMem = new InMemItem { DateTimeValue = DateTime.Today.AddDays(1) };

			_memoryCompareDateTime.MeetsCondition(inMem, GetQueryParameter("DateTimeValue", DateTime.Today, Comparisons.GreaterEqualThan)).ShouldBeTrue();
		}

		[Fact]
		public void CanCompareLessEqualThanWithSameNumbers_MeetsConditionIsTrue()
		{
			var inMem = new InMemItem { DateTimeValue = DateTime.Today };

			_memoryCompareDateTime.MeetsCondition(inMem, GetQueryParameter("DateTimeValue", DateTime.Today, Comparisons.LessEqualThan)).ShouldBeTrue();
		}

		[Fact]
		public void CanCompareLessEqualThan_MeetsConditionIsTrue()
		{
			var inMem = new InMemItem { DateTimeValue = DateTime.Today.AddDays(-1) };

			_memoryCompareDateTime.MeetsCondition(inMem, GetQueryParameter("DateTimeValue", DateTime.Today, Comparisons.LessEqualThan)).ShouldBeTrue();
		}

		[Fact]
		public void CanCompareLessEqualThan_MeetsConditionIsFalse()
		{
			var inMem = new InMemItem { DateTimeValue = DateTime.Today.AddDays(1) };

			_memoryCompareDateTime.MeetsCondition(inMem, GetQueryParameter("DateTimeValue", DateTime.Today, Comparisons.LessEqualThan)).ShouldBeFalse();
		}

		[Fact]
		public void CanCompareLessThan_MeetsConditionIsTrue()
		{
			var inMem = new InMemItem { DateTimeValue = DateTime.Today.AddDays(-1) };

			_memoryCompareDateTime.MeetsCondition(inMem, GetQueryParameter("DateTimeValue", DateTime.Today, Comparisons.LessThan)).ShouldBeTrue();
		}

		[Fact]
		public void CanCompareLessThan_MeetsConditionIsFalse()
		{
			var inMem = new InMemItem { DateTimeValue = DateTime.Today.AddDays(1) };

			_memoryCompareDateTime.MeetsCondition(inMem, GetQueryParameter("DateTimeValue", DateTime.Today, Comparisons.LessThan)).ShouldBeFalse();
		}
	}
}
