using Eklee.Azure.Functions.GraphQl.Example.HttpMocks;
using Eklee.Azure.Functions.GraphQl.Example.Models;
using Eklee.Azure.Functions.GraphQl.Repository;
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
				.Use<Book, InMemoryRepository>()
				.Build();

			inputBuilderFactory.Create<Reviewer>(this)
				.Use<Reviewer, InMemoryRepository>()
				.Build();

			inputBuilderFactory.Create<Author>(this)
				.Use<Author, InMemoryRepository>()
				.Build();

			inputBuilderFactory.Create<BookAuthors>(this)
				.Use<BookAuthors, InMemoryRepository>()
				.Use<Author, InMemoryRepository>()
				.Use<Book, InMemoryRepository>()
				.Build();

			inputBuilderFactory.Create<BookReview>(this)
				.Use<BookReview, InMemoryRepository>()
				.Build();

			const string publishersResource = "publishers";

			inputBuilderFactory.Create<Publisher>(this)
				.Use<Publisher, HttpRepository>()
				.ConfigureHttp()
					.AddBaseUrl("http://localhost:7071")
					.AddResource(publishersResource, "POST")
					.UpdateResource(publishersResource, "PUT")
					.DeleteResource(publishersResource, "DELETE")
					.Build()
				.Build();
		}
	}
}
