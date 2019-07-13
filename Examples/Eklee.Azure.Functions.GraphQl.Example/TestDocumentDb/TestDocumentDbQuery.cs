using System;
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

			queryBuilderFactory.Create<Model7>(this, "GetModel7WithModel8Id", "Get Model8")
				.WithParameterBuilder()
				.WithConnectionEdgeBuilder<Model7ToModel8>()
					.WithDestinationId()
				.BuildConnectionEdgeParameters()
				.BuildQuery()
				.BuildWithListResult();
		}
	}
}
