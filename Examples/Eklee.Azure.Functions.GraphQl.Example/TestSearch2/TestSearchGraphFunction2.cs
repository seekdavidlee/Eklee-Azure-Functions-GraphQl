using System.Threading.Tasks;
using Eklee.Azure.Functions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Eklee.Azure.Functions.GraphQl.Example.TestSearch2
{
	public static class TestSearchGraphFunction2
	{
		[ExecutionContextDependencyInjection(typeof(TestSearchModule2))]
		[FunctionName("TestSearch2")]
		public static async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "testsearch2/graph")] HttpRequest req,
			ILogger log,
			ExecutionContext executionContext)
		{
			return await executionContext.ProcessGraphQlRequest(req);
		}
	}
}
