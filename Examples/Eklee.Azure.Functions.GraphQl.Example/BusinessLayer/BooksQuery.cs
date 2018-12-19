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

			this.AddBookAuthorsOutputQueries(queryBuilderFactory);

			this.AddPublisherQueries(queryBuilderFactory);

			this.AddBookReviewQueries(queryBuilderFactory);
		}
	}
}
