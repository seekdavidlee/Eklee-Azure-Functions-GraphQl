using System.Net.Http;
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

			//inputBuilderFactory.Create<BookReview>(this)
			//	.Use<BookReview, DocumentDbRepository>()
			//	.ConfigureDocumentDb()
			//		.AddUrl("https://localhost:8081")
			//		.AddKey("C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==")
			//		.Build()
			//	.Build();

			const string publishersResource = "publishers";

			inputBuilderFactory.Create<Publisher>(this)
				.Use<Publisher, HttpRepository>()
				.ConfigureHttp()
					.AddBaseUrl("http://localhost:7071/api/")
					.AddResource(publisher => new HttpResource { AppendUrl = publishersResource, Method = HttpMethod.Post })
					.UpdateResource(publisher => new HttpResource { AppendUrl = $"{publishersResource}/{publisher.Id}", Method = HttpMethod.Put })
					.DeleteResource(publisher => new HttpResource { AppendUrl = $"{publishersResource}/{publisher.Id}", Method = HttpMethod.Delete })
					.QueryResource(items => new HttpQueryResource
					{
						AppendUrl = $"{publishersResource}/{items["id"]}",
						QueryType = HttpQueryTypes.AppendToUrl,
						ForQueryName = PublisherQueryExtensions.GetPublisherByIdQuery
					})
					.Build()
				.Build();
		}
	}
}
