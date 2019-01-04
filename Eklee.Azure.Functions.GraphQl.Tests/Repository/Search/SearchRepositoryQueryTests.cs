using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Eklee.Azure.Functions.GraphQl.Repository.Search;
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

	public class SearchBookModel
	{
		public string SearchText { get; set; }
	}

	public class SearchRepositoryQueryTests : SearchRepositoryQueryTestsBase
	{
		private readonly SearchRepository _searchRepository;

		[Fact]
		public async Task CanFindBook()
		{
			await SeedAsync();

			var queryParameters = new List<QueryParameter>();

			queryParameters.Add(new QueryParameter
			{

			});

			var items = new Dictionary<string, object>
			{
				[SearchConstants.QueryTypes] = new[] { typeof(SearchBook), typeof(SearchReviewer) }
			};

			var results = await SearchRepository.QueryAsync<SearchBook>("test1", queryParameters, items);
		}
	}
}
