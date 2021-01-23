using Eklee.Azure.Functions.GraphQl.Repository.Search;
using Eklee.Azure.Functions.GraphQl.Repository.Search.Filters;
using FastMember;
using Shouldly;
using System;
using System.Linq;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests.Repository.Search.Filters
{
	[Trait(Constants.Category, Constants.UnitTests)]
	public class NumericSearchFilterTests
	{
		private readonly MemberSet _members;
		public NumericSearchFilterTests()
		{
			_members = TypeAccessor.Create(typeof(MockItem)).GetMembers();
		}

		[Fact]
		public void CanHandleString()
		{
			var filter = new NumericSearchFilter();
			filter.CanHandle(Comparisons.Equal, _members.Single(m => m.Name == "StringValue")).ShouldBeFalse();
		}

		[Fact]
		public void CannotHandleInt()
		{
			var filter = new NumericSearchFilter();
			filter.CanHandle(Comparisons.Equal, _members.Single(m => m.Name == "IntValue")).ShouldBeTrue();
		}

		[Fact]
		public void CanGenerateEquals()
		{
			var filter = new NumericSearchFilter();
			filter.GetFilter(new SearchFilterModel
			{
				FieldName = "IntValue",
				Comprison = Comparisons.Equal,
				Value = "1"
			}, _members.Single(m => m.Name == "IntValue")).ShouldNotBeNullOrEmpty();
		}

		[Fact]
		public void CanGenerateNotEquals()
		{
			var filter = new NumericSearchFilter();
			filter.GetFilter(new SearchFilterModel
			{
				FieldName = "IntValue",
				Comprison = Comparisons.NotEqual,
				Value = "2"
			}, _members.Single(m => m.Name == "IntValue")).ShouldNotBeNullOrEmpty();
		}

		[Fact]
		public void CanGenerateGreaterThan()
		{
			var filter = new NumericSearchFilter();
			filter.GetFilter(new SearchFilterModel
			{
				FieldName = "IntValue",
				Comprison = Comparisons.GreaterThan,
				Value = "3"
			}, _members.Single(m => m.Name == "IntValue")).ShouldNotBeNullOrEmpty();
		}

		[Fact]
		public void CanGenerateGreaterEqualThan()
		{
			var filter = new NumericSearchFilter();
			filter.GetFilter(new SearchFilterModel
			{
				FieldName = "IntValue",
				Comprison = Comparisons.GreaterEqualThan,
				Value = "4"
			}, _members.Single(m => m.Name == "IntValue")).ShouldNotBeNullOrEmpty();
		}

		[Fact]
		public void CanGenerateLessEqualThan()
		{
			var filter = new NumericSearchFilter();
			filter.GetFilter(new SearchFilterModel
			{
				FieldName = "IntValue",
				Comprison = Comparisons.LessEqualThan,
				Value = "5"
			}, _members.Single(m => m.Name == "IntValue")).ShouldNotBeNullOrEmpty();
		}

		[Fact]
		public void CanGenerateLessThan()
		{
			var filter = new NumericSearchFilter();
			filter.GetFilter(new SearchFilterModel
			{
				FieldName = "IntValue",
				Comprison = Comparisons.LessThan,
				Value = "6"
			}, _members.Single(m => m.Name == "IntValue")).ShouldNotBeNullOrEmpty();
		}

		[Fact]
		public void DateTimeStringIsODataCompliant()
		{
			var filter = new NumericSearchFilter();

			TypeAccessor ta = TypeAccessor.Create(typeof(TestNumericSearchFilter));
			var m = ta.GetMembers().Single(x => x.Name == "MyDateTime");
			var result = filter.GetFilter(new SearchFilterModel
			{
				Comprison = Comparisons.Equal,
				FieldName = "MyDateTime",
				Value = "2019-08-02"
			}, m);

			result.ShouldBe("MyDateTime eq 2019-08-02T00:00:00Z");
		}

		[Fact]
		public void DateTimeOffsetStringIsODataCompliant()
		{
			var filter = new NumericSearchFilter();

			TypeAccessor ta = TypeAccessor.Create(typeof(TestNumericSearchFilter));
			var m = ta.GetMembers().Single(x => x.Name == "MyDateTimeOffset");
			var result = filter.GetFilter(new SearchFilterModel
			{
				Comprison = Comparisons.Equal,
				FieldName = "MyDateTimeOffset",
				Value = "2017-01-12"
			}, m);

			result.ShouldBe("MyDateTimeOffset eq 2017-01-12T00:00:00Z");
		}
	}

	public class TestNumericSearchFilter
	{
		public DateTime MyDateTime { get; set; }
		public DateTime MyDateTimeOffset { get; set; }
	}
}
