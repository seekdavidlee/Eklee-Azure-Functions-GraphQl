using System.Threading.Tasks;
using Eklee.Azure.Functions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Eklee.Azure.Functions.GraphQl.Example.TestSearch
{
	public static class TestSearchGraphFunction
	{
		[ExecutionContextDependencyInjection(typeof(TestSearchModule))]
		[FunctionName("TestSearch")]
		public static async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "testsearch/graph")] HttpRequest req,
			ILogger log,
			ExecutionContext executionContext)
		{
			return await executionContext.ProcessGraphQlRequest(req);
		}
	}
}
