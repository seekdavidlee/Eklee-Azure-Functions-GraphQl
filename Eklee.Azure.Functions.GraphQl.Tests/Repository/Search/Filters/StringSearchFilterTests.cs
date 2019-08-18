using Eklee.Azure.Functions.GraphQl.Repository.Search;
using Eklee.Azure.Functions.GraphQl.Repository.Search.Filters;
using FastMember;
using Shouldly;
using System.Linq;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests.Repository.Search.Filters
{
	[Trait(Constants.Category, Constants.UnitTests)]
	public class StringSearchFilterTests
	{
		private readonly MemberSet _members;
		public StringSearchFilterTests()
		{
			_members = TypeAccessor.Create(typeof(MockItem)).GetMembers();
		}

		[Fact]
		public void CanHandleString()
		{
			var filter = new StringSearchFilter();
			filter.CanHandle(Comparisons.Equal, _members.Single(m => m.Name == "StringValue")).ShouldBeTrue();
		}

		[Fact]
		public void CannotHandleInt()
		{
			var filter = new StringSearchFilter();
			filter.CanHandle(Comparisons.Equal, _members.Single(m => m.Name == "IntValue")).ShouldBeFalse();
		}

		[Fact]
		public void CanGenerateEquals()
		{
			var filter = new StringSearchFilter();
			filter.GetFilter(new SearchFilterModel
			{
				FieldName = "StringValue",
				Comprison = Comparisons.Equal,
				Value = "One"
			}, _members.Single(m => m.Name == "StringValue")).ShouldNotBeNullOrEmpty();
		}

		[Fact]
		public void CanGenerateNotEquals()
		{
			var filter = new StringSearchFilter();
			filter.GetFilter(new SearchFilterModel
			{
				FieldName = "StringValue",
				Comprison = Comparisons.NotEqual,
				Value = "One"
			}, _members.Single(m => m.Name == "StringValue")).ShouldNotBeNullOrEmpty();
		}
	}
}
