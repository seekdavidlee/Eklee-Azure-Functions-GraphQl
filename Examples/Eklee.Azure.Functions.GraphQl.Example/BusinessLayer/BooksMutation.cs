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
				.Delete<BookId, Status>(
					bookInput => new Book { Id = bookInput.Id },
					book => new Status { Message = $"Successfully removed book with Id {book.Id}" })
				.Use<Book, InMemoryRepository>()
				.DeleteAll(() => new Status { Message = "All books have been removed." })
				.Build();

			// Typically, you want to store these settings somewhere safe and access it from services like Azure KeyVault. Since
			// this is local setting which is static, I am using it directly.

			const string documentDbKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
			const string documentDbUrl = "https://localhost:8081";

			inputBuilderFactory.Create<Reviewer>(this)
				.Delete<ReviewerId, Status>(
					reviewerInput => new Reviewer { Id = reviewerInput.Id },
					bookReview => new Status { Message = $"Successfully removed reviewer with Id {bookReview.Id}" })
				.Use<Reviewer, DocumentDbRepository>()
				.ConfigureDocumentDb()
					.AddUrl(documentDbUrl)
					.AddKey(documentDbKey)
					.AddDatabase(rc => "local")
					.AddRequestUnit(400)
					.AddPartition(reviewer => reviewer.Region)
					.Build()
				.DeleteAll(() => new Status { Message = "All reviewers have been removed." })    // Used more for local development to reset local database than having any operational value.
				.Build();

			inputBuilderFactory.Create<Author>(this)
				.Use<Author, InMemoryRepository>()
				.DeleteAll(() => new Status { Message = "All authors have been removed." })
				.Build();

			inputBuilderFactory.Create<BookAuthors>(this)
				.Use<BookAuthors, InMemoryRepository>()
				.Use<Author, InMemoryRepository>()
				.Use<Book, InMemoryRepository>()
				.DeleteAll(() => new Status { Message = "All book authors relationships have been removed." })
				.Build();

			inputBuilderFactory.Create<BookReview>(this)
				.Delete<BookReviewId, Status>(
					bookReviewInput => new BookReview { Id = bookReviewInput.Id, BookId = bookReviewInput.BookId },
					bookReview => new Status { Message = $"Successfully removed book review with Id {bookReview.Id}" })
				.Use<BookReview, DocumentDbRepository>()
				.ConfigureDocumentDb()
					.AddUrl(documentDbUrl)
					.AddKey(documentDbKey)
					.AddDatabase(rc => "local")
					.AddRequestUnit(400)
					.AddPartition(bookReview => bookReview.BookId)
					.Build()
				.Build();

			const string publishersResource = "publishers";

			inputBuilderFactory.Create<Publisher>(this)
				.Use<Publisher, HttpRepository>()
				.ConfigureHttp()
					.AddBaseUrl("http://localhost:7071/api/")
					.AddResource(publisher => new HttpResource { AppendUrl = publishersResource, Method = HttpMethod.Post })
					.UpdateResource(publisher => new HttpResource { AppendUrl = $"{publishersResource}/{publisher.Id}", Method = HttpMethod.Put })
					.DeleteResource(publisher => new HttpResource { AppendUrl = $"{publishersResource}/{publisher.Id}", Method = HttpMethod.Delete })
					.QueryResource(PublisherQueryExtensions.GetPublisherByIdQuery, items => new HttpQueryResource
					{
						AppendUrl = $"{publishersResource}/{items["id"]}",
						QueryType = HttpQueryTypes.AppendToUrl
					})
					.DeleteAllResource(() => new HttpResource { AppendUrl = publishersResource, Method = HttpMethod.Delete })
					.BuildHttp()
				.DeleteAll(() => new Status { Message = "All publishers have been removed." })
				.Build();
		}
	}
}
