using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Eklee.Azure.Functions.Http;
using FastMember;
using GraphQL;
using GraphQL.Builders;
using GraphQL.Http;
using GraphQL.Types;
using GraphQL.Types.Relay;
using GraphQL.Types.Relay.DataObjects;
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

        public static void EnableGraphQlPagingFor<TGraphType>(this ContainerBuilder builder) where TGraphType : IGraphType
        {
            builder.RegisterType<ConnectionType<TGraphType>>();
            builder.RegisterType<PageInfoType>();
            builder.RegisterType<EdgeType<TGraphType>>();
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
        /// <typeparam name="T">T to resolve.</typeparam>
        /// <param name="graphQlCache">Cache implementation.</param>
        /// <param name="getResult">Func to get the result if we are unable to get the result from cache.</param>
        /// <param name="cacheDurationInSeconds">Cache duration in seconds.</param>
        /// <param name="contextKey">Key in which we can use to interrogate the context with.</param>
        /// <returns></returns>
        public static Func<ResolveFieldContext<object>, object> ResolverWithCache<T>(
            this IGraphQlCache graphQlCache, Func<object, T> getResult, int cacheDurationInSeconds, string contextKey = null)
        {
            // Ref: https://graphql.github.io/learn/caching/

            return context => graphQlCache.GetByKeyAsync(getResult,
                contextKey != null ? context.GetArgument<object>(contextKey) : context.DiscoverContextValueByKey<T>(), cacheDurationInSeconds).Result;
        }

        public static async Task<Connection<T>> GetConnectionWithCacheAsync<T>(this ResolveConnectionContext<object> context,
            IGraphQlCache graphQlCache,
            Func<object, IEnumerable<T>> getResults,
            string contextKey,
            int defaultPageLimit = 10, int cacheDurationInSeconds = 10)
        {
            var items = await graphQlCache.GetByKeyAsync(getResults, context.GetArgument<object>(contextKey),
                     cacheDurationInSeconds);

            return await context.GetConnectionAsync(items, defaultPageLimit);
        }

        /// <summary>
        /// Gets Connection.
        /// </summary>
        /// <typeparam name="T">T item.</typeparam>
        /// <param name="context">Connection context.</param>
        /// <param name="enumerable">An IEnumerable of T items.</param>
        /// <param name="defaultPageLimit">Default page limit.</param>
        /// <returns></returns>
        public static async Task<Connection<T>> GetConnectionAsync<T>(this ResolveConnectionContext<object> context, IEnumerable<T> enumerable, int defaultPageLimit = 10)
        {
            // Ref: https://graphql.org/learn/pagination/

            var list = enumerable.ToList();

            var total = list.Count;
            int skip = 0;
            int take = defaultPageLimit;

            var edges = list.Select(TransformToEdge).ToList();

            if (!string.IsNullOrEmpty(context.After))
            {
                skip = GetIndex(context.After) + 1;
            }

            if (context.First.HasValue)
            {
                take = context.First.Value;
            }

            var hasNextPage = total > skip + take;

            var filtered = edges.Skip(skip).Take(take).ToList();

            var first = filtered.FirstOrDefault().GetCursor();

            var connection = new Connection<T>
            {
                TotalCount = total,
                Edges = filtered,
                PageInfo = new PageInfo
                {
                    StartCursor = first,
                    EndCursor = filtered.LastOrDefault().GetCursor(),
                    HasNextPage = hasNextPage,
                    HasPreviousPage = GetIndex(first) > 0
                }
            };

            return await Task.FromResult(connection);
        }

        private static string GetCursor<T>(this Edge<T> edge)
        {
            return edge?.Cursor;
        }

        private static Edge<T> TransformToEdge<T>(T item, int index)
        {
            var cursor = Convert.ToBase64String(Encoding.UTF8.GetBytes(index.ToString()));

            return new Edge<T> { Cursor = cursor, Node = item };
        }

        private static int GetIndex(string cursor)
        {
            return !string.IsNullOrEmpty(cursor) ? Convert.ToInt32(Encoding.UTF8.GetString(Convert.FromBase64String(cursor))) : -1;
        }

	    public static string GetCacheKey(this List<QueryParameter> list)
	    {
		    return string.Join("_", list.Select(x => x.ContextValue.Value.ToString()));
	    }

	    public static string GetDescription(this Member member)
	    {
		    var description = (DescriptionAttribute)member.GetAttribute(typeof(DescriptionAttribute), false);
		    return description != null ? description.Description : "";
	    }

	    public static Member GetMember(this List<Member> members, string name)
	    {
		    return members.Single(x => x.Name.ToLower() == name);
	    }

	    public static void Add(this List<ModelMember> list, string name, bool isOptional)
	    {
		    list.Add(new ModelMember { Name = name, IsOptional = isOptional });
	    }

	    public static ContextValue GetContextValue(this Dictionary<string, object> args, string name)
	    {
		    var contextValue = new ContextValue { IsNotSet = !args.ContainsKey(name) };

		    if (!contextValue.IsNotSet)
		    {
			    contextValue.Value = args[name];
		    }

		    return contextValue;
	    }

	    public static string GetKey<T>(this T item)
	    {
		    var f = TypeAccessor.Create(typeof(T));
		    return string.Join("", f.GetMembers().Where(x => x.GetAttribute(typeof(KeyAttribute), false) != null)
			    .Select(x => f[item, x.Name].ToString()));
	    }
	}
}
