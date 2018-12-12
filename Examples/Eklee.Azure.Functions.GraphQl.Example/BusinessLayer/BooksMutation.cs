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
				.Use<Book, InMemoryDbRepository>()
				.Build();

			inputBuilderFactory.Create<Reviewer>(this)
				.Use<Reviewer, InMemoryDbRepository>()
				.Build();

			inputBuilderFactory.Create<Author>(this)
				.Use<Author, InMemoryDbRepository>()
				.Build();

			inputBuilderFactory.Create<BookAuthors>(this)
				.Use<BookAuthors, InMemoryDbRepository>()
				.Use<Author, InMemoryDbRepository>()
				.Use<Book, InMemoryDbRepository>()
				.Build();

			inputBuilderFactory.Create<BookReview>(this)
				.Use<BookReview, InMemoryDbRepository>()
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
