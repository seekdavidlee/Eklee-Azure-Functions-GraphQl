using System;
using System.Collections.Generic;
using System.Linq;
using Eklee.Azure.Functions.GraphQl.Example.Models;
using GraphQL.Types;
using Microsoft.Extensions.Logging;

namespace Eklee.Azure.Functions.GraphQl.Example.TestDocumentDb
{
	public class TestDocumentDbQuery : ObjectGraphType<object>
	{
		public TestDocumentDbQuery(QueryBuilderFactory queryBuilderFactory, ILogger logger)
		{
			logger.LogInformation("Creating document db queries.");

			Name = "query";

			queryBuilderFactory.Create<Model1>(this, "searchModel1", "Search for a single Model 1 by Id")
				.WithCache(TimeSpan.FromSeconds(15))
				.WithParameterBuilder()
				.WithProperty(x => x.Id)
				.BuildQuery()
				.BuildWithSingleResult();

			queryBuilderFactory.Create<Model2>(this, "searchModel2", "Search for a single Model 2 by Id")
				.WithCache(TimeSpan.FromSeconds(15))
				.WithParameterBuilder()
				.WithProperty(x => x.Id)
				.BuildQuery()
				.BuildWithSingleResult();

			queryBuilderFactory.Create<Model5>(this, "searchModel5", "Search for a single Model 5 by Id")
				.WithParameterBuilder()
				.WithProperty(x => x.Id)
				.BuildQuery()
				.BuildWithSingleResult();

			queryBuilderFactory.Create<Model6>(this, "searchModel6", "Search for a single Model 6 by Id")
				.WithParameterBuilder()
				.WithProperty(x => x.Id)
				.BuildQuery()
				.BuildWithSingleResult();

			queryBuilderFactory.Create<Model6>(this, "searchAllModel6", "Get all Model 6")
				.WithParameterBuilder()
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
			queryBuilderFactory.Create<Model7>(this, "GetModel7WithModel8Field", "Get Model7 With Model8 Field")
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
					.BuildConnectionEdgeParameters(ctx =>
					{
						ctx.Items["model7IdList"] = ctx.GetQueryResults<Model7ToModel8>().Select(x => (object)x.Id).ToList();
					})
				.ThenWithQuery<Model7>()
				.WithPropertyFromSource(x => x.Id, ctx =>
				{
					return (List<object>)ctx.Items["model7IdList"];
				})
				.BuildQueryResult(ctx => { })
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

		}
	}
}
