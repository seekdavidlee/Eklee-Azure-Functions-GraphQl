using System;
using System.Linq;
using System.Net.Http;
using Eklee.Azure.Functions.GraphQl.Example.HttpMocks;
using Eklee.Azure.Functions.GraphQl.Example.Models;
using Eklee.Azure.Functions.GraphQl.Repository.Http;
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
				.ConfigureInMemory<Book>().BuildInMemory()
				.DeleteAll(() => new Status { Message = "All books have been removed." })
				.Build();

			// You want to store these settings somewhere safe and access using services like Azure KeyVault.

			string documentDbKey = configuration["DocumentDb:Key"];
			string documentDbUrl = configuration["DocumentDb:Url"];

			var tenants = configuration.GetSection("Tenants").GetChildren().ToList();
			tenants.ForEach(tenant =>
			{
				var issuer = tenant["Issuer"];
				string tenantDocumentDbKey = tenant["DocumentDb:Key"];
				string tenantDocumentDbUrl = tenant["DocumentDb:Url"];
				int tenantRequestUnits = Convert.ToInt32(tenant["DocumentDb:RequestUnits"]);

				inputBuilderFactory.Create<Reviewer>(this)
					.Delete<ReviewerId, Status>(
						reviewerInput => new Reviewer { Id = reviewerInput.Id },
						bookReview => new Status { Message = $"Successfully removed reviewer with Id {bookReview.Id}" })
					.ConfigureDocumentDb<Reviewer>()
					.AddGraphRequestContextSelector(ctx => ctx.ContainsIssuer(issuer))
					.AddUrl(tenantDocumentDbUrl)
					.AddKey(tenantDocumentDbKey)
					.AddDatabase(issuer.GetTenantIdFromIssuer())
					.AddRequestUnit(tenantRequestUnits)
					.AddPartition(reviewer => reviewer.Region)
					.BuildDocumentDb()
					.DeleteAll(() => new Status { Message = "All reviewers have been removed." })    // Used more for local development to reset local database than having any operational value.
					.Build();
			});

			inputBuilderFactory.Create<Author>(this)
				.ConfigureInMemory<Author>().BuildInMemory()
				.DeleteAll(() => new Status { Message = "All authors have been removed." })
				.Build();

			inputBuilderFactory.Create<BookAuthors>(this)
				.ConfigureInMemory<BookAuthors>().BuildInMemory()
				.ConfigureInMemory<Author>().BuildInMemory()
				.ConfigureInMemory<Book>().BuildInMemory()
				.DeleteAll(() => new Status { Message = "All book authors relationships have been removed." })
				.Build();

			inputBuilderFactory.Create<BookReview>(this)
				.Delete<BookReviewId, Status>(
					bookReviewInput => new BookReview { Id = bookReviewInput.Id },
					bookReview => new Status { Message = $"Successfully removed book review with Id {bookReview.Id}" })
				.ConfigureDocumentDb<BookReview>()
					.AddUrl(documentDbUrl)
					.AddKey(documentDbKey)
					.AddDatabase("local")
					.AddRequestUnit(400)
					.AddPartition(bookReview => bookReview.ReviewerId)
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


			inputBuilderFactory.Create<BookPrice>(this)
				.Delete<BookPriceId, Status>(
					input => new BookPrice { Id = input.Id },
					input => new Status { Message = $"Successfully removed book review with Id {input.Id}" })
				.ConfigureDocumentDb<BookPrice>()
				.AddUrl(documentDbUrl)
				.AddKey(documentDbKey)
				.AddDatabase("local")
				.AddRequestUnit(400)
				.AddPartition(input => input.Type)
				.BuildDocumentDb()
				.DeleteAll(() => new Status { Message = "All book price relationships have been removed." })
				.Build();

			inputBuilderFactory.Create<BookSearch>(this)
				.DeleteAll(() => new Status { Message = "All book searches have been deleted." })
				.ConfigureSearch<BookSearch>()
				.AddApiKey(configuration["Search:ApiKey"])
				.AddServiceName(configuration["Search:ServiceName"])
				.BuildSearch()
				.Build();

			inputBuilderFactory.Create<ReviewerSearch>(this)
				.DeleteAll(() => new Status { Message = "All reviewer searches have been deleted." })
				.ConfigureSearchWith<ReviewerSearch, Reviewer>()
				.AddApiKey(configuration["Search:ApiKey"])
				.AddServiceName(configuration["Search:ServiceName"])
				.AddPrefix("stg")
				.BuildSearch()
				.Build();
		}
	}
}
