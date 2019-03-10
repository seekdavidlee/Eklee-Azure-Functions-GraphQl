using System;
using Eklee.Azure.Functions.GraphQl.Example.Models;
using GraphQL.Types;
using Microsoft.Extensions.Logging;

namespace Eklee.Azure.Functions.GraphQl.Example.TestInMemory
{
	public class TestInMemoryQuery : ObjectGraphType<object>
	{
		public TestInMemoryQuery(QueryBuilderFactory queryBuilderFactory, ILogger logger)
		{
			logger.LogInformation("Creating in memory queries.");

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
		}
	}
}
