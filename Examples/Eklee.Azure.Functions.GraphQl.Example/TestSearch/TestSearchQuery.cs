using System;
using System.Collections.Generic;
using System.Linq;
using Eklee.Azure.Functions.GraphQl.Example.TestSearch.Models;
using Eklee.Azure.Functions.GraphQl.Repository.Search;
using GraphQL.Types;
using Microsoft.Extensions.Logging;

namespace Eklee.Azure.Functions.GraphQl.Example.TestSearch
{
	public class TestSearchQuery : ObjectGraphType<object>
	{
		public TestSearchQuery(QueryBuilderFactory queryBuilderFactory, ILogger logger)
		{
			logger.LogInformation("Creating queries.");

			Name = "query";

			queryBuilderFactory.Create<MySearchResult>(this, "search1", "Searches across Models.")
				.WithCache(TimeSpan.FromSeconds(10))
				.WithParameterBuilder()
				.BeginSearch(typeof(MySearch1), typeof(MySearch2), typeof(MySearch3))
				.BuildQueryResult(ctx =>
				{
					var searches = ctx.GetQueryResults<SearchResultModel>();
					var search1 = searches.GetTypeList<MySearch1>();
					var search2 = searches.GetTypeList<MySearch2>();
					var search3 = searches.GetTypeList<MySearch3>();

					ctx.Items["search1"] = search1;
					ctx.Items["search1IdList"] = search1.Select(x => (object)x.Id).ToList();

					ctx.Items["search2"] = search1;
					ctx.Items["search2IdList"] = search2.Select(x => (object)x.Id).ToList();

					ctx.Items["search3"] = search1;
					ctx.Items["search3IdList"] = search3.Select(x => (object)x.Id).ToList();
				})
				.ThenWithQuery<Model1>()
				.WithPropertyFromSource(x => x.Id, ctx => (List<object>)ctx.Items["search1IdList"]).BuildQueryResult(x =>
				{
					var results = x.GetQueryResults<Model1>();
					x.SetResults(results.Select(r => new MySearchResult
					{
						DateField = r.DateField,
						DoubleField = r.DoubleField,
						Field = r.Field,
						Id = r.Id,
						IntField = r.IntField
					}).ToList());
				})
				.ThenWithQuery<Model2>()
				.WithPropertyFromSource(x => x.Id, ctx => (List<object>)ctx.Items["search2IdList"]).BuildQueryResult(x =>
				{
					var results = x.GetQueryResults<Model2>();
					var currentResults = results.Select(r => new MySearchResult
					{
						DateField = r.DateField,
						DoubleField = r.DoubleField,
						Field = r.Field,
						Id = r.Id,
						IntField = r.IntField
					}).ToList();
					currentResults.AddRange(x.GetResults<MySearchResult>());
					x.SetResults(currentResults);
				})
				.ThenWithQuery<Model3>()
				.WithPropertyFromSource(x => x.Id, ctx => (List<object>)ctx.Items["search3IdList"]).BuildQueryResult(x =>
				{
					var results = x.GetQueryResults<Model3>();
					var currentResults = results.Select(r => new MySearchResult
					{
						DateField = r.DateField,
						DoubleField = r.DoubleField,
						Field = r.Field,
						Id = r.Id,
						IntField = r.IntField
					}).ToList();
					currentResults.AddRange(x.GetResults<MySearchResult>());
					x.SetResults(currentResults);
				})
				.BuildQuery().BuildWithListResult();

		}
	}
}
