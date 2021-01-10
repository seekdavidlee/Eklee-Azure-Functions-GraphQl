using System.Threading.Tasks;
using Eklee.Azure.Functions.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Eklee.Azure.Functions.GraphQl.Example
{
    public static class BooksGraphFunction
    {
        [ExecutionContextDependencyInjection(typeof(MyModule))]
        [FunctionName("graph")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "books/graph")] HttpRequest req,
            ILogger log,
            ExecutionContext executionContext)
        {
            return await executionContext.ProcessGraphQlRequest(req);
        }
    }

    public static class PagingBooksGraphFunction
    {
        [ExecutionContextDependencyInjection(typeof(MyPagingBooksModule))]
        [FunctionName("paginggraph")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "pagingbooks/graph")] HttpRequest req,
            ILogger log,
            ExecutionContext executionContext)
        {
            return await executionContext.ProcessGraphQlRequest(req);
        }
    }
}
