using GraphQL.Types;
using Microsoft.Extensions.Logging;

namespace Eklee.Azure.Functions.GraphQl.Example.BusinessLayer
{
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
