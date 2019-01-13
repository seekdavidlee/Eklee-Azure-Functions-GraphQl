﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eklee.Azure.Functions.GraphQl.Repository.DocumentDb;
using Eklee.Azure.Functions.GraphQl.Repository.Search;
using FastMember;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Eklee.Azure.Functions.GraphQl.Tests.Repository.Search
{
	public abstract class SearchRepositoryQueryTestsBase : IDisposable
	{
		protected readonly SearchRepository SearchRepository;

		protected readonly List<SearchBook> SearchBooks = new List<SearchBook>();
		protected readonly List<SearchReviewer> SearchReviewers = new List<SearchReviewer>();

		protected SearchRepositoryQueryTestsBase()
		{
			var searchConfig = LocalConfiguration.Get().GetSection("Search");
			var logger = Substitute.For<ILogger>();
			SearchRepository = new SearchRepository(logger);

			SeedSearchBooks(searchConfig);
			SeedBookReviewers(searchConfig);
		}

		private void SeedSearchBooks(IConfigurationSection searchConfig)
		{
			Dictionary<string, object> configurations = new Dictionary<string, object>();

			configurations.Add<SearchBook>(SearchConstants.ApiKey, searchConfig[SearchConstants.ApiKey]);
			configurations.Add<SearchBook>(SearchConstants.ServiceName, searchConfig[SearchConstants.ServiceName]);

			SearchRepository.Configure(typeof(SearchBook), configurations);

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
				Name = "American Cool Techs",
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

		private void SeedBookReviewers(IConfigurationSection searchConfig)
		{
			Dictionary<string, object> configurations = new Dictionary<string, object>();

			configurations.Add<SearchReviewer>(SearchConstants.ApiKey, searchConfig[SearchConstants.ApiKey]);
			configurations.Add<SearchReviewer>(SearchConstants.ServiceName, searchConfig[SearchConstants.ServiceName]);

			SearchRepository.Configure(typeof(SearchReviewer), configurations);

			SearchReviewers.Add(new SearchReviewer
			{
				Id = Guid.NewGuid(),
				Active = true,
				Name = "James Lee"
			});

			SearchReviewers.Add(new SearchReviewer
			{
				Id = Guid.NewGuid(),
				Active = true,
				Name = "Art Tops"
			});
		}

		protected async Task SeedAsync()
		{
			await SearchRepository.BatchAddAsync(SearchBooks);
			await SearchRepository.BatchAddAsync(SearchReviewers);

			// May need to give some time for indexing to catch up.
			Thread.Sleep(1000);
		}

		protected async Task<IEnumerable<SearchResultModel>> SearchAsync(
			string searchText, TimeSpan timeout, int expectedCount, params Type[] types)
		{
			var type = typeof(SearchModel);
			var accessor = TypeAccessor.Create(type);
			var member = accessor.GetMembers().Single(x => x.Name == "SearchText");

			// Indicates to search on what types to query on.
			var items = new Dictionary<string, object>
			{
				[SearchConstants.QueryTypes] = types
			};

			List<SearchResultModel> results = new List<SearchResultModel>();
			DateTime start = DateTime.UtcNow;

			while (results.Count == 0)
			{
				results = (await SearchRepository.QueryAsync<SearchResultModel>("test1", new[]
				{
					new QueryParameter
					{
						ContextValue = new ContextValue { Value = searchText, Comparison = Comparisons.Equal},
						MemberModel = new ModelMember(type,accessor,member,false)
					}
				}, items)).ToList();

				if (results.Count >= expectedCount) break;

				if (timeout == TimeSpan.Zero) break;

				TimeSpan span = DateTime.UtcNow - start;
				if (span > timeout) break;

				Thread.Sleep(500);
			}

			return results;
		}

		public void Dispose()
		{
			SearchRepository.DeleteAllAsync<SearchBook>().GetAwaiter().GetResult();
			SearchRepository.DeleteAllAsync<SearchReviewer>().GetAwaiter().GetResult();
		}
	}
}
