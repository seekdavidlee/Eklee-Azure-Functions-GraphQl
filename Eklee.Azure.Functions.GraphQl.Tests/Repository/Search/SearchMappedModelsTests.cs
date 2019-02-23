using System;
using System.Collections.Generic;
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

	public class Model1WithMore
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public int Counter { get; set; }
		public List<string> Categories { get; set; }
	}

	public class Model1SearchWithLess
	{
		public string Id { get; set; }
		public string Name { get; set; }
	}

	public class Model2WithNoneStringFields
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public int Counter { get; set; }
		public double Price { get; set; }
	}

	public class Model2SearchWithOnlyStringFields
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Counter { get; set; }
		public string Price { get; set; }
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

		[Fact]
		public void ShouldMappedAllSearchModelPropertiesFromModelWithMoreFields()
		{
			_searchMappedModels.Map<Model1SearchWithLess, Model1WithMore>();

			var o = (Model1SearchWithLess)_searchMappedModels.CreateInstanceFromMap(new Model1WithMore { Id = "foo 45", Name = "foo 3331", Counter = 33, Categories = new List<string> { "1", "2" } });
			o.Id.ShouldBe("foo 45");
			o.Name.ShouldBe("foo 3331");
		}

		[Fact]
		public void ShouldMappedAllSearchModelNoneStringProperties()
		{
			_searchMappedModels.Map<Model2SearchWithOnlyStringFields, Model2WithNoneStringFields>();

			var o = (Model2SearchWithOnlyStringFields)_searchMappedModels.CreateInstanceFromMap(new Model2WithNoneStringFields { Id = "foo5511", Name = "foo 41", Counter = 32423, Price = 3245.99 });
			o.Id.ShouldBe("foo5511");
			o.Name.ShouldBe("foo 41");
			o.Counter.ShouldBe("32423");
			o.Price.ShouldBe("3245.99");
		}
	}
}
