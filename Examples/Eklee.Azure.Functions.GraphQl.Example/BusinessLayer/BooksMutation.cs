using Eklee.Azure.Functions.GraphQl.Example.Models;
using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl.Example.BusinessLayer
{
	public class BooksMutation : ObjectGraphType
	{
		public BooksMutation(InputBuilderFactory inputBuilderFactory)
		{
			Name = "mutations";

			inputBuilderFactory.Create<Book>(this)
				.Delete<BookId, Status>(book => new Status { Message = $"Successfully removed book with Id {book.Id}" })
				.Build();

			inputBuilderFactory.Create<Reviewer>(this).Build();

			inputBuilderFactory.Create<Author>(this).Build();

			inputBuilderFactory.Create<BookAuthors>(this).Build();

			inputBuilderFactory.Create<BookReview>(this).Build();
		}
	}
}
