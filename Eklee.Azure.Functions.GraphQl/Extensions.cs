using System.Threading.Tasks;
using Autofac;
using Eklee.Azure.Functions.Http;
using GraphQL;
using GraphQL.Http;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;

namespace Eklee.Azure.Functions.GraphQl
{
    public static class Extensions
    {
        public static void RegisterGraphQl<TSchema>(this ContainerBuilder builder) where TSchema : ISchema
        {
            builder.RegisterType<GraphQlDomain>().As<IGraphQlDomain>().SingleInstance();
            builder.RegisterType<DocumentExecuter>().As<IDocumentExecuter>().SingleInstance();
            builder.RegisterType<DocumentWriter>().As<IDocumentWriter>().SingleInstance();
            builder.RegisterType<TSchema>().As<ISchema>().SingleInstance();
            builder.RegisterType<GraphDependencyResolver>().As<IDependencyResolver>();
        }

        public static async Task<IActionResult> ProcessGraphQlRequest(this ExecutionContext executionContext, HttpRequest httpRequest)
        {
            var graphQlDomain = executionContext.Resolve<IGraphQlDomain>();

            if (httpRequest.ContentType == "application/json")
            {
                var requestBody = await httpRequest.ReadAsStringAsync();

                return new OkObjectResult(await graphQlDomain.ExecuteAsync(JsonConvert.DeserializeObject<GraphQlDomainRequest>(requestBody)));
            }

            // TODO: Log request Content-Type is unsupported.

            return new BadRequestResult();
        }
    }
}
