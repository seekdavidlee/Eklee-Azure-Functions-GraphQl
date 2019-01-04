using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eklee.Azure.Functions.GraphQl.Repository;
using Eklee.Azure.Functions.GraphQl.Repository.Search;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Eklee.Azure.Functions.GraphQl.Tests.Repository.Search
{
	public abstract class SearchRepositoryQueryTestsBase
	{
		protected readonly SearchRepository SearchRepository;

		protected readonly List<SearchBook> SearchBooks = new List<SearchBook>();

		protected SearchRepositoryQueryTestsBase()
		{
			var searchConfig = LocalConfiguration.Get().GetSection("Search");
			var logger = Substitute.For<ILogger>();
			SearchRepository = new SearchRepository(logger);

			Dictionary<string, object> configurations = new Dictionary<string, object>();

			configurations.Add<SearchBook>(SearchConstants.ApiKey, searchConfig[SearchConstants.ApiKey]);
			configurations.Add<SearchBook>(SearchConstants.ServiceName, searchConfig[SearchConstants.ServiceName]);

			SearchRepository.Configure(typeof(SearchBook), configurations);

			AddSearchBooks();
		}

		private void AddSearchBooks()
		{
			SearchBooks.Add(new SearchBook
			{
				Id = Guid.NewGuid(),
				Active = true,
				Name = "I Love this Book 1",
				Category = "Tech",
				ListPrice = 54.99M,
				Published = new DateTime(2011, 1, 4)
			});

			SearchBooks.Add(new SearchBook
			{
				Id = Guid.NewGuid(),
				Active = true,
				Name = "Cool",
				Category = "Tech",
				ListPrice = 91.16M,
				Published = new DateTime(2000, 8, 1)
			});

			SearchBooks.Add(new SearchBook
			{
				Id = Guid.NewGuid(),
				Active = true,
				Name = "Ro Bot Book",
				Category = "Tech",
				ListPrice = 54.99M,
				Published = new DateTime(2011, 1, 4)
			});

			SearchBooks.Add(new SearchBook
			{
				Id = Guid.NewGuid(),
				Active = false,
				Name = "Hello World",
				Category = "Fiction",
				ListPrice = 54.99M,
				Published = new DateTime(2011, 1, 4)
			});

			SearchBooks.Add(new SearchBook
			{
				Id = Guid.NewGuid(),
				Active = true,
				Name = "Cats and Dogs",
				Category = "Fiction",
				ListPrice = 14.19M,
				Published = new DateTime(2011, 1, 4)
			});
			SearchBooks.Add(new SearchBook
			{
				Id = Guid.NewGuid(),
				Active = true,
				Name = "Better stocks",
				Category = "Business",
				ListPrice = 2.99M,
				Published = new DateTime(2011, 1, 4)
			});
			SearchBooks.Add(new SearchBook
			{
				Id = Guid.NewGuid(),
				Active = false,
				Name = "American Car Makers",
				Category = "Cars",
				ListPrice = 122.19M,
				Published = new DateTime(2009, 12, 9)
			});
			SearchBooks.Add(new SearchBook
			{
				Id = Guid.NewGuid(),
				Active = true,
				Name = "American History III",
				Category = "History",
				ListPrice = 56,
				Published = new DateTime(2001, 5, 14)
			});

			SearchBooks.Add(new SearchBook
			{
				Id = Guid.NewGuid(),
				Active = true,
				Name = "ART 123",
				Category = "Art",
				ListPrice = 90.10M,
				Published = new DateTime(1988, 10, 14)
			});
		}

		protected async Task SeedAsync()
		{
			await SearchRepository.BatchAddAsync(SearchBooks);
		}

		protected async Task<IEnumerable<T>> SearchAsync<T>(T item) where T : class
		{
			//var type = typeof(T);
			//var accessor = TypeAccessor.Create(type);
			//var member = accessor.GetMembers().Single(x => x.Name == "Id");

			//return await DocumentDbRepository.QueryAsync<T>("test1", new[]
			//{
			//	new QueryParameter
			//	{
			//		ContextValue = new ContextValue { Value = id, Comparison = Comparisons.Equal},
			//		MemberModel = new ModelMember(type, accessor, member, false)
			//	}
			//});

			throw new NotImplementedException();
		}
	}
}
