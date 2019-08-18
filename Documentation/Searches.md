[Main page](../README.md)

# Searches
We can have the ability to search across multiple types and ochestrate into a common type.

## Search Aggregations
The following shows an example of a search using BeginSearch which lets us add types to search on. One important feature here is the option to BuildWithAggregate which lets us display aggregates. For more information, refer here: https://docs.microsoft.com/en-us/azure/search/search-faceted-navigation.

We should apply the IsFacetable to define aggregation on the appropriate property on Search Model and IsFilterable to filter on that property.

```
queryBuilderFactory.Create<MySearchResult2>(this, "searchWithAggregate", "Searches across Models.")
	.WithCache(TimeSpan.FromSeconds(10))
	.WithParameterBuilder()
	.BeginSearch()
		.Add<MySearch3>()
		.BuildWithAggregate()
	.BuildQueryResult(ctx =>
	{
		var searches = ctx.GetSystemItems<SearchResult>();
		var output = new MySearchResult2();

		var idList = new List<string>();
		searches.ForEach(search =>
		{
			idList.AddRange(search.Values.Select(y => ((MySearch3)y.Value).Id));
			output.Aggregates.AddRange(search.Aggregates);
		});

		ctx.Items["Output"] = output;
		ctx.Items["search3IdList"] = idList.Select(id => (object)id).ToList();
	})
	.ThenWithQuery<Model3V2>()
	.WithPropertyFromSource(x => x.Id, ctx => (List<object>)ctx.Items["search3IdList"]).BuildQueryResult(ctx =>
	{
		var results = ctx.GetQueryResults<Model3V2>();
		var output = (MySearchResult2)ctx.Items["Output"];
		output.Results = results.Select(r => new MySearchResult
		{
			DateField = r.DateField,
			DoubleField = r.DoubleField,
			Field = r.Field,
			Id = r.Id,
			IntField = r.IntField
		}).ToList();

		ctx.SetResults(new List<MySearchResult2> { output });
	}).BuildQuery().BuildWithSingleResult();
```

## Search Aggregations Filtering

We can also filter based on the value from search aggregate so that the user can drill down on their search. The following types are supported:

* String (Note that Azure Search supports Equals and Not Equals comparisons. StartsWith, EndsWith and Contains are not supported.)
* Double
* Int
* DateTime

```
query {
  searchWithAggregate(searchtext:{equal:"orange"},filters:[
    {
      fieldName:"FieldTwo"
      fieldValue:"katy"
      comparison:notequal
    }
  ]){
    results {
      id
      field
    }
    aggregates {
      fieldName
      fieldAggregates {
        count
        value
      }
    }
  }
}
```
