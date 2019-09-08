using Eklee.Azure.Functions.GraphQl.Example.TestSearch2.Models;
using Eklee.Azure.Functions.GraphQl.Repository.Search;
using GraphQL.Types;
using Microsoft.Extensions.Logging;

namespace Eklee.Azure.Functions.GraphQl.Example.TestSearch2
{
	public class TestSearchQuery2 : ObjectGraphType<object>
	{
		public TestSearchQuery2(QueryBuilderFactory queryBuilderFactory, ILogger logger)
		{
			logger.LogInformation("Creating queries.");

			Name = "query";

			queryBuilderFactory.Create<MySearch4>(this, "searchModel4", "Search for all Model4")
				.WithParameterBuilder()
				.BeginSearch()
				.Add<MySearch4>()
				.BuildWithAggregate()
				.BuildQueryResult(ctx =>
				{
					var searches = ctx.GetQueryResults<SearchResultModel>();
					ctx.SetResults(searches.GetTypeList<MySearch4>());
				}).BuildQuery().BuildWithListResult();
		}
	}
}
