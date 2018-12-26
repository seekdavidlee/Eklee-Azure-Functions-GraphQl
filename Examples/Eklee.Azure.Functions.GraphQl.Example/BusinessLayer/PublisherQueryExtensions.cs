using System;
using Eklee.Azure.Functions.GraphQl.Example.HttpMocks;

namespace Eklee.Azure.Functions.GraphQl.Example.BusinessLayer
{
	public static class PublisherQueryExtensions
	{
		public const string GetPublisherByIdQuery = "getPublisherById";
		public static void AddPublisherQueries(this BooksQuery booksQuery, QueryBuilderFactory queryBuilderFactory)
		{
			queryBuilderFactory.Create<Publisher>(booksQuery, GetPublisherByIdQuery)
				.WithCache(TimeSpan.FromSeconds(10))
					.WithParameterBuilder()
					.WithProperty(x => x.Id)
					.BuildQuery()
				.BuildWithSingleResult();
		}
	}
}
