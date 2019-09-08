using Eklee.Azure.Functions.GraphQl.Example.TestSearch2.Models;
using Eklee.Azure.Functions.GraphQl.Repository.Search;
using GraphQL.Types;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Eklee.Azure.Functions.GraphQl.Example.TestSearch2
{
	public class TestSearchQuery2 : ObjectGraphType<object>
	{
		public TestSearchQuery2(QueryBuilderFactory queryBuilderFactory, ILogger logger)
		{
			logger.LogInformation("Creating queries.");

			Name = "query";

			queryBuilderFactory.Create<MySearch4Result>(this, "searchModel4", "Search for all Model4")
				.WithParameterBuilder()
				.BeginSearch()
					.Add<MySearch4>()
				.BuildWithAggregate()
				.BuildQueryResult(ctx =>
				{
					var searches = ctx.GetQueryResults<SearchResultModel>();

					var result = new MySearch4Result();
					result.Results = searches.GetTypeList<MySearch4>();
					result.Aggregates = new List<SearchAggregateModel>();

					var agg = ctx.GetSystemItems<SearchResult>();
					agg.ForEach(a => result.Aggregates.AddRange(a.Aggregates));

					ctx.SetResults(new List<MySearch4Result> { result });

				}).BuildQuery().BuildWithSingleResult();
		}
	}
}
