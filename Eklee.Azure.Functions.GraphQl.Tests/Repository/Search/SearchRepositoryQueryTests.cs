using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests.Repository.Search
{
	public class SearchBook
	{
		[Key]
		public Guid Id { get; set; }
		public string Name { get; set; }
		public string Category { get; set; }
		public decimal ListPrice { get; set; }
		public DateTime Published { get; set; }
		public bool Active { get; set; }
	}

	public class SearchReviewer
	{
		[Key]
		public Guid Id { get; set; }
		public string Name { get; set; }
		public bool Active { get; set; }
	}

	[Trait(Constants.Category, Constants.IntegrationTests)]
	public class SearchRepositoryQueryTests : SearchRepositoryQueryTestsBase
	{
		[Fact]
		public async Task CanSearchAcrossTypesWhereOnlyOneTypeHasResults()
		{
			await SeedAsync();

			var results = (await SearchAsync("American",
				TimeSpan.FromSeconds(30), 3, typeof(SearchBook), typeof(SearchReviewer))).ToList();

			results.Count.ShouldBe(3);

			var books = results.Select(x => x.Value as SearchBook).Where(x => x != null).ToList();
			books.Count.ShouldBe(3);

			var reviewers = results.Select(x => x.Value as SearchReviewer).Where(x => x != null).ToList();
			reviewers.Count.ShouldBe(0);

			books.SingleOrDefault(x => x.Name == "American Cool Techs").ShouldNotBeNull();
			books.SingleOrDefault(x => x.Name == "American Car Makers").ShouldNotBeNull();
			books.SingleOrDefault(x => x.Name == "American History III").ShouldNotBeNull();
		}

		[Fact]
		public async Task CanSearchAcrossTypes()
		{
			await SeedAsync();

			var results = (await SearchAsync("Art",
				TimeSpan.FromSeconds(30), 2, typeof(SearchBook), typeof(SearchReviewer))).ToList();

			results.Count.ShouldBe(2);

			var books = results.Select(x => x.Value as SearchBook).Where(x => x != null).ToList();
			books.Count.ShouldBe(1);

			var reviewers = results.Select(x => x.Value as SearchReviewer).Where(x => x != null).ToList();
			reviewers.Count.ShouldBe(1);

			books.SingleOrDefault(x => x.Name == "ART 123").ShouldNotBeNull();
			reviewers.SingleOrDefault(x => x.Name == "Art Tops").ShouldNotBeNull();

		}
	}
}
