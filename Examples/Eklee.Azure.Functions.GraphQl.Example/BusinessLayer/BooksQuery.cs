using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl.Example.BusinessLayer
{
	public class BooksQuery : ObjectGraphType<object>
	{
		public BooksQuery(QueryBuilderFactory queryBuilderFactory)
		{
			Name = "query";

			this.AddBooksQueries(queryBuilderFactory);

			this.AddBookAuthorsOutputQueries(queryBuilderFactory);

			this.AddPublisherQueries(queryBuilderFactory);
		}
	}
}
