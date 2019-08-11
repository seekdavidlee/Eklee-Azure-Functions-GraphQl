using Eklee.Azure.Functions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Example.Operations
{
	public static class OperationsHttpFunction
	{
		[ExecutionContextDependencyInjection(typeof(OperationsModule))]
		[FunctionName("DeleteSearchIndexes")]
		public static async Task<IActionResult> Add(
		[HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "searchIndexes")] HttpRequest req,
		ILogger log,
		ExecutionContext executionContext)
		{
			var operations = executionContext.Resolve<IOperations>();
			await operations.DeleteSearchIndexes();
			return new OkResult();
		}
	}
}
