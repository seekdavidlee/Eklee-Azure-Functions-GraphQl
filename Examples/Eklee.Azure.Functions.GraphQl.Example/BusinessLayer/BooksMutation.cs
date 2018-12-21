using System.Net.Http;
using Eklee.Azure.Functions.GraphQl.Example.HttpMocks;
using Eklee.Azure.Functions.GraphQl.Example.Models;
using Eklee.Azure.Functions.GraphQl.Repository;
using GraphQL.Types;
using Microsoft.Extensions.Configuration;

namespace Eklee.Azure.Functions.GraphQl.Example.BusinessLayer
{
	public class BooksMutation : ObjectGraphType
	{
		public BooksMutation(InputBuilderFactory inputBuilderFactory, IConfiguration configuration)
		{
			Name = "mutations";

			inputBuilderFactory.Create<Book>(this)
				.Delete<BookId, Status>(
					bookInput => new Book { Id = bookInput.Id },
					book => new Status { Message = $"Successfully removed book with Id {book.Id}" })
				.Use<Book, InMemoryRepository>()
				.DeleteAll(() => new Status { Message = "All books have been removed." })
				.Build();

			// Typically, you want to store these settings somewhere safe and access it from services like Azure KeyVault.

			string documentDbKey = configuration["DocumentDb:Key"];
			string documentDbUrl = configuration["DocumentDb:Url"];

			inputBuilderFactory.Create<Reviewer>(this)
				.Delete<ReviewerId, Status>(
					reviewerInput => new Reviewer { Id = reviewerInput.Id },
					bookReview => new Status { Message = $"Successfully removed reviewer with Id {bookReview.Id}" })
				.ConfigureDocumentDb<Reviewer>()
					.AddUrl(documentDbUrl)
					.AddKey(documentDbKey)
					.AddDatabase(rc => "local")
					.AddRequestUnit(400)
					.AddPartition(reviewer => reviewer.Region)
					.BuildDocumentDb()
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
				.ConfigureDocumentDb<BookReview>()
					.AddUrl(documentDbUrl)
					.AddKey(documentDbKey)
					.AddDatabase(rc => "local")
					.AddRequestUnit(400)
					.AddPartition(bookReview => bookReview.BookId)
					.BuildDocumentDb()
				.DeleteAll(() => new Status { Message = "All book reviews relationships have been removed." })
				.Build();

			const string publishersResource = "publishers";

			inputBuilderFactory.Create<Publisher>(this)				
				.ConfigureHttp<Publisher>()
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
