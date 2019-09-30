using GraphQL.Types;
using Eklee.Azure.Functions.GraphQl.Example.Models;
using System.Collections.Generic;
using System.Linq;

namespace Eklee.Azure.Functions.GraphQl.Example.TestStorage.Core
{
	public class TestStorageQueryConfigObjectGraphType : ObjectGraphType<object>
	{
		public TestStorageQueryConfigObjectGraphType(QueryBuilderFactory queryBuilderFactory)
		{
			Name = "query";

			queryBuilderFactory.Create<Model7>(this, "GetModel7WithIdFromHeader", "Get Model7")
				.WithParameterBuilder()
					.WithConnectionEdgeBuilder<Model7ToModel8>()
						.WithSourceIdFromSource<Model7>(ctx => new List<object> { (string)ctx.RequestContext.HttpRequest.Request.Headers["Model7IdKey"] })
					.BuildConnectionEdgeParameters()
				.BuildQuery()
				.BuildWithListResult();

			queryBuilderFactory.Create<Model7>(this, "GetModel7WithModel8Id", "Get Model7")
				.WithParameterBuilder()
				.WithConnectionEdgeBuilder<Model7ToModel8>()
					.WithDestinationId()
				.BuildConnectionEdgeParameters()
				.BuildQuery()
				.BuildWithListResult();


			// Find Model7 coming from Model8
			queryBuilderFactory.Create<Model7>(this, "GetModel7WithModel8FieldAndConnectionFieldDescription", "Get Model7 With Model8 Field And Connection Field Description")
				.WithParameterBuilder()
				.BeginQuery<Model8>()
				// Use field from Model8 as a starting point to search from.
				.WithProperty(x => x.Field)
				.BuildQueryResult(ctx =>
				{
					ctx.Items["model8IdList"] = ctx.GetQueryResults<Model8>().Select(x => (object)x.Id).ToList();
				})
				.WithConnectionEdgeBuilder<Model7ToModel8>()
					// Now, we match Model7ToModel8's DestinationId with Model8 Id.
					.WithDestinationIdFromSource(ctx =>
					{
						return (List<object>)ctx.Items["model8IdList"];
					})
					.WithProperty(x => x.FieldDescription)
					.BuildConnectionEdgeParameters(ctx =>
					{
						ctx.Items["model7IdList"] = ctx.GetQueryResults<Model7ToModel8>().Select(x => x.Id)
							.Distinct()
							.Select(x => (object)x).ToList();
					})
				.ThenWithQuery<Model7>()
				.WithPropertyFromSource(x => x.Id, ctx =>
				{
					return (List<object>)ctx.Items["model7IdList"];
				})
				.BuildQueryResult(ctx => { })
				.BuildQuery()
				.BuildWithListResult();

			queryBuilderFactory.Create<Model9>(this, "GetModel9ById")
				.WithParameterBuilder()
				.WithKeys()
				.BuildQuery()
				.BuildWithListResult();

			queryBuilderFactory.Create<Model9>(this, "GetModel9")
				.WithParameterBuilder()
				.BeginQuery<Model9>()
					.WithPropertyFromSource(x => x.Id, ctx => new List<object> { "model9_2" })
					.WithPropertyFromSource(x => x.Field, ctx => new List<object> { "model9 2" })
				.BuildQueryResult(ctx => ctx.SetResults(ctx.GetQueryResults<Model9>()))
				.BuildQuery()
				.BuildWithListResult();

			queryBuilderFactory.Create<Model9>(this, "GetModel9Foo")
				.WithParameterBuilder()
				.BeginQuery<Model9>()
					.WithPropertyFromSource(x => x.Id, ctx => new List<object> { "model9_2_foo" })
					.WithPropertyFromSource(x => x.Field, ctx => new List<object> { "model9 2" })
				.BuildQueryResult(ctx => ctx.SetResults(ctx.GetQueryResults<Model9>()))
				.BuildQuery()
				.BuildWithListResult();

			queryBuilderFactory.Create<Model13Parent>(this, "GetModel13Parent")
				.WithParameterBuilder()
				.BeginQuery<Model13Parent>()
					.WithPropertyFromSource(x => x.AccountId, ctx =>
					{
						var accountId = ctx.RequestContext.HttpRequest.Request.Headers["AccountId"].Single();
						ctx.Items["AccountIdList"] = new List<object> { accountId };
						return new List<object> { accountId };
					})
				.BuildQueryResult(ctx =>
				{
					ctx.Items["IdList"] = ctx.GetQueryResults<Model13Parent>().Select(x => (object)x.SomeKey).ToList();
				})
				.WithConnectionEdgeBuilder<Model13Edge>()
					.WithSourceIdFromSource<Model13Parent>(ctx => (List<object>)ctx.Items["IdList"])
					.ForDestinationFilter<Model13Child>(x => x.AccountId, ctx => (List<object>)ctx.Items["AccountIdList"])
				.BuildConnectionEdgeParameters()
				.BuildQuery()
				.BuildWithListResult();

			queryBuilderFactory.Create<Model14>(this, "GetModel14ById")
				.WithParameterBuilder()
				.WithKeys()
				.BuildQuery()
				.BuildWithListResult();
		}
	}
}
