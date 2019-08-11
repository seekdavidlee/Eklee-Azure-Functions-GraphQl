using Eklee.Azure.Functions.GraphQl.Queries;
using Eklee.Azure.Functions.GraphQl.Repository.Search;
using FastMember;
using Shouldly;
using System;
using System.Linq;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests.Queries
{
	[Trait(Constants.Category, Constants.UnitTests)]
	public class SearchFilterModelQueryArgumentTests
	{
		[Fact]
		public void CanHandleSearchModelFilters()
		{
			Type t = typeof(SearchModel);
			TypeAccessor ta = TypeAccessor.Create(t);
			var member = ta.GetMembers().ToList().Single(x => x.Name == "Filters");
			var filtersMemberModel = new ModelMember(null, null, member, false);

			var qa = new SearchFilterModelQueryArgument();
			qa.CanHandle(filtersMemberModel).ShouldBeTrue();
		}
	}
}
