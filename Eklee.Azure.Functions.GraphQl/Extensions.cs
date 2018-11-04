using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Eklee.Azure.Functions.Http;
using FastMember;
using GraphQL;
using GraphQL.Http;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Caching.Distributed;
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

        public static void EnableGraphQlCache<TDistributedCache>(this ContainerBuilder builder) where TDistributedCache : IDistributedCache
        {
            builder.RegisterType<GraphQlCache>().As<IGraphQlCache>().SingleInstance();
            builder.UseDistributedCache<TDistributedCache>();
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

        /// <summary>
        /// Attempt to discover the context key from referring to the type which will contain at least one KeyAttribute.
        /// </summary>
        /// <typeparam name="T">"T "Type.</typeparam>
        /// <param name="context">Context.</param>
        /// <returns></returns>
        public static object DiscoverContextValueByKey<T>(this ResolveFieldContext<object> context)
        {
            var t = typeof(T);
            var accessor = TypeAccessor.Create(t);

            var member = accessor.GetMembers().FirstOrDefault(x => x.GetAttribute(typeof(KeyAttribute), false) != null);
            if (member != null)
            {
                return context.GetArgument<object>(member.Name.ToLower());
            }

            throw new ArgumentException($"Type {t.Name} does not contain a property with Key attribute.");
        }

        /// <summary>
        /// Creates a Resolver instance that can be used to satisfy the criteria for resolution of the T instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="graphQlCache">Cache implementation.</param>
        /// <param name="getResult">Func to get the result if we are unable to get the result from cache.</param>
        /// <param name="cacheDurationInSeconds">Cache duration in seconds.</param>
        /// <param name="contextKey">Key in which we can use to interrogate the context with.</param>
        /// <returns></returns>
        public static Func<ResolveFieldContext<object>, object> ResolverWithCache<T>(
            this IGraphQlCache graphQlCache, Func<object, T> getResult, int cacheDurationInSeconds, string contextKey = null)
        {
            return context => graphQlCache.GetByKeyAsync(getResult,
                contextKey != null ? context.GetArgument<object>(contextKey) : context.DiscoverContextValueByKey<T>(), cacheDurationInSeconds).Result;
        }
    }
}
