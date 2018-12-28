using System.Linq;
using Eklee.Azure.Functions.GraphQl.Example.HttpMocks;
using Eklee.Azure.Functions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Eklee.Azure.Functions.GraphQl.Example
{
	public static class MyPublisherHttpFunction
	{
		[ExecutionContextDependencyInjection(typeof(MyModule2))]
		[FunctionName("addPublisher")]
		public static IActionResult Add(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "publishers")] HttpRequest req,
			ILogger log,
			ExecutionContext executionContext)
		{
			var ctx = executionContext.Resolve<IHttpRequestContext>();
			var publisher = executionContext.Resolve<IHttpMockRepository<Publisher>>();
			var item = ctx.GetModelFromBody<Publisher>();
			publisher.Add(item);
			return new RedirectResult($"http://{req.Host}{req.Path}/{item.Id}");
		}


		[ExecutionContextDependencyInjection(typeof(MyModule2))]
		[FunctionName("updatePublisher")]
		public static IActionResult Update(
			[HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "publishers/{id}")] HttpRequest req,
			ILogger log,
			string id,
			ExecutionContext executionContext)
		{
			var ctx = executionContext.Resolve<IHttpRequestContext>();
			var instance = ctx.GetModelFromBody<Publisher>();

			if (instance.Id != id)
			{
				return new BadRequestResult();
			}

			var publisher = executionContext.Resolve<IHttpMockRepository<Publisher>>();
			publisher.Update(instance);
			return new OkObjectResult(instance);
		}

		[ExecutionContextDependencyInjection(typeof(MyModule2))]
		[FunctionName("deletePublisher")]
		public static IActionResult Delete(
			[HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "publishers/{id}")] HttpRequest req,
			ILogger log,
			string id,
			ExecutionContext executionContext)
		{
			var publisher = executionContext.Resolve<IHttpMockRepository<Publisher>>();
			publisher.Delete(id);
			return new OkResult();
		}

		[ExecutionContextDependencyInjection(typeof(MyModule2))]
		[FunctionName("getPublisher")]
		public static IActionResult Get(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "publishers/{id}")] HttpRequest req,
			ILogger log,
			string id,
			ExecutionContext executionContext)
		{
			var publisher = executionContext.Resolve<IHttpMockRepository<Publisher>>();
			return new OkObjectResult(publisher.Search().Single(x => x.Id == id));
		}

		[ExecutionContextDependencyInjection(typeof(MyModule2))]
		[FunctionName("deleteAllPublisher")]
		public static IActionResult DeleteAll(
			[HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "publishers")] HttpRequest req,
			ILogger log,
			ExecutionContext executionContext)
		{
			var publisher = executionContext.Resolve<IHttpMockRepository<Publisher>>();
			publisher.ClearAll();
			return new OkResult();
		}
	}
}
