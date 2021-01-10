using Eklee.Azure.Functions.GraphQl.Example.Models;
using GraphQL.Types;
using Microsoft.Extensions.Logging;

namespace Eklee.Azure.Functions.GraphQl.Example.BusinessLayer
{
	public class BooksQuery : ObjectGraphType<object>
	{
		public BooksQuery(QueryBuilderFactory queryBuilderFactory, ILogger logger)
		{
			logger.LogInformation("Creating queries.");

			Name = "query";

			this.AddBooksQueries(queryBuilderFactory);

			//this.AddBookAuthorsOutputQueries(queryBuilderFactory);

			this.AddPublisherQueries(queryBuilderFactory);

			this.AddBookReviewQueries(queryBuilderFactory);

			queryBuilderFactory.Create<Author>(this, "getAuthorByHomeCity")
				.WithParameterBuilder()
				.WithProperty(x => x.HomeCity)
				.BuildQuery()
				.BuildWithListResult();
		}
	}

	public class PagingBooksQuery : ObjectGraphType<object>
	{
		public PagingBooksQuery(QueryBuilderFactory queryBuilderFactory, ILogger logger)
		{
			logger.LogInformation("Creating queries.");

			Name = "query";

			this.AddBookAuthorsOutputQueries(queryBuilderFactory);
		}
	}
}
