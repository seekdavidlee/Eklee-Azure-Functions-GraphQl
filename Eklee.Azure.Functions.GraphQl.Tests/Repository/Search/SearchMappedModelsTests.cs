using System;
using System.ComponentModel.DataAnnotations;
using Eklee.Azure.Functions.GraphQl.Repository.Search;
using Shouldly;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests.Repository.Search
{
	public class MyTestFooSearch
	{
		[Key]
		public string Id { get; set; }

		public string Name { get; set; }

		public int Age { get; set; }

		public DateTime Created { get; set; }

		public decimal Cost { get; set; }
	}

	public class MyTestFoo : MyTestFooSearch
	{

	}

	public class MyOtherFoo : MyTestFooSearch
	{

	}

	[Trait(Constants.Category, Constants.UnitTests)]
	public class SearchMappedModelsTests
	{
		private readonly SearchMappedModels _searchMappedModels = new SearchMappedModels();

		[Fact]
		public void ShouldFindMappedSearchTypeWhenMapped()
		{
			_searchMappedModels.Map<MyTestFooSearch, MyTestFoo>();

			Type searchType;
			_searchMappedModels.TryGetMappedSearchType<MyTestFoo>(out searchType).ShouldBe(true);
			searchType.ShouldNotBeNull();
		}

		[Fact]
		public void ShouldNotFindMappedSearchTypeWhenNotMapped()
		{
			_searchMappedModels.Map<MyTestFooSearch, MyTestFoo>();

			Type searchType;
			_searchMappedModels.TryGetMappedSearchType<MyOtherFoo>(out searchType).ShouldBe(false);
			searchType.ShouldBeNull();
		}

		[Fact]
		public void ShouldMappedAllSearchModelProperties()
		{
			_searchMappedModels.Map<MyTestFooSearch, MyTestFoo>();

			var o = (MyTestFooSearch)_searchMappedModels.CreateInstanceFromMap(new MyTestFoo { Id = "foo", Name = "foo 1", Age = 33, Cost = 34.67M, Created = new DateTime(2014, 1, 13) });
			o.Name.ShouldBe("foo 1");
			o.Age.ShouldBe(33);
			o.Cost.ShouldBe(34.67M);
			o.Created.Year.ShouldBe(2014);
			o.Created.Month.ShouldBe(1);
			o.Created.Day.ShouldBe(13);
		}
	}
}
