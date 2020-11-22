using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Eklee.Azure.Functions.GraphQl.Example.TestStorage.Core;
using Eklee.Azure.Functions.Http;

namespace Eklee.Azure.Functions.GraphQl.Example.TestStorage
{
	public static class GraphQLFunction
	{
		[ExecutionContextDependencyInjection(typeof(TestStorageFunctionModule))]
		[FunctionName("TestStorage")]
		public static async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "teststorage/graph")] HttpRequest req,
			ILogger log,
			ExecutionContext executionContext)
		{
			return await executionContext.ProcessGraphQlRequest(req);
		}
	}
}
