using Eklee.Azure.Functions.GraphQl.Repository.Search;
using Eklee.Azure.Functions.GraphQl.Repository.Search.Filters;
using FastMember;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests.Repository.Search.Filters
{

	[Trait(Constants.Category, Constants.UnitTests)]
	public class SearchFilterProviderTests
	{
		private readonly TypeAccessor _searchModeltypeAccessor;
		private readonly Type _searchModelType;
		private readonly List<QueryParameter> _queryParams = new List<QueryParameter>();
		private readonly MemberSet _members;
		public SearchFilterProviderTests()
		{
			var mockItemType = typeof(MockItem);
			var mockItemTypeAccessor = TypeAccessor.Create(mockItemType);

			_members = mockItemTypeAccessor.GetMembers();
			_searchModelType = typeof(SearchModel);
			_searchModeltypeAccessor = TypeAccessor.Create(_searchModelType);
		}

		[Fact]
		public void NoMatchShouldReturnNull()
		{
			var searchFilterProvider = new SearchFilterProvider(null);
			searchFilterProvider.GenerateStringFilter(_queryParams, _members).ShouldBeNull();
		}

		[Fact]
		public void ThrowArgumentExceptionIfInputNotValid()
		{
			AddQueryParameterWithValidModelMember(new ContextValue
			{
				Values = new List<object>
				{
					new SearchFilterModel
					{
						FieldName = "Foo"
					}
				}
			});

			var searchFilterProvider = new SearchFilterProvider(null);
			Should.Throw<ArgumentException>(() => searchFilterProvider.GenerateStringFilter(_queryParams, _members));
		}

		private void AddQueryParameterWithValidModelMember(ContextValue contextValue)
		{
			var member = _searchModeltypeAccessor.GetMembers().Single(m => m.Name == "Filters");
			var queryParameter = new QueryParameter
			{
				MemberModel = new ModelMember(_searchModelType, _searchModeltypeAccessor, member, false),
				ContextValue = contextValue
			};

			_queryParams.Add(queryParameter);
		}

		[Fact]
		public void ShouldReturnNullIfNoMatchingFiltersFound()
		{
			var searchFilterProvider = new SearchFilterProvider(new List<ISearchFilter>());

			searchFilterProvider.GenerateStringFilter(_queryParams, _members).ShouldBeNull();
		}

		private SearchFilterProvider GetSearchFilterProviderWithFilters()
		{
			return new SearchFilterProvider(new List<ISearchFilter>
			{
				new StringSearchFilter()
			});
		}

		[Fact]
		public void ShouldReturnIfStringFilterMatched()
		{
			AddQueryParameterWithValidModelMember(new ContextValue
			{
				Values = new List<object>
				{
					new SearchFilterModel
					{
						FieldName = "StringValue",
						Comprison = Comparisons.Equal,
						Value = "One"
					}
				}
			});

			var searchFilterProvider = GetSearchFilterProviderWithFilters();
			searchFilterProvider.GenerateStringFilter(_queryParams, _members).ShouldNotBeNullOrEmpty();
		}
	}
}
