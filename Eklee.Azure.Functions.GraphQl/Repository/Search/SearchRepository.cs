using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eklee.Azure.Functions.GraphQl.Repository.DocumentDb;
using Eklee.Azure.Functions.GraphQl.Repository.Search.Filters;
using Microsoft.Extensions.Logging;

namespace Eklee.Azure.Functions.GraphQl.Repository.Search
{
	public class SearchRepository : IGraphQlRepository
	{
		private readonly ILogger _logger;
		private readonly ISearchFilterProvider _searchFilterProvider;
		private readonly List<SearchClientProvider> _providers = new List<SearchClientProvider>();

		public SearchRepository(ILogger logger, ISearchFilterProvider searchFilterProvider)
		{
			_logger = logger;
			_searchFilterProvider = searchFilterProvider;
		}

		public void Configure(Type sourceType, Dictionary<string, object> configurations)
		{
			var serviceName = configurations.GetStringValue(SearchConstants.ServiceName, sourceType);

			SearchClientProvider searchClientProvider = _providers.SingleOrDefault(p => p.ContainsServiceName(serviceName));

			if (searchClientProvider == null)
			{
				searchClientProvider = new SearchClientProvider(_searchFilterProvider, _logger, serviceName, configurations.GetStringValue(SearchConstants.ApiKey, sourceType));
				_providers.Add(searchClientProvider);
			}

			searchClientProvider.ConfigureSearchService(configurations, sourceType);
		}

		private SearchClientProvider GetProvider<T>(IGraphRequestContext graphRequestContext) where T : class
		{
			return _providers.Single(x => x.CanHandle<T>(graphRequestContext));
		}

		public async Task BatchAddAsync<T>(IEnumerable<T> items, IGraphRequestContext graphRequestContext) where T : class
		{
			await GetProvider<T>(graphRequestContext).BatchCreateAsync(items, graphRequestContext);
		}

		public async Task AddAsync<T>(T item, IGraphRequestContext graphRequestContext) where T : class
		{
			await GetProvider<T>(graphRequestContext).CreateAsync(item, graphRequestContext);
		}

		public async Task UpdateAsync<T>(T item, IGraphRequestContext graphRequestContext) where T : class
		{
			await GetProvider<T>(graphRequestContext).UpdateAsync(item, graphRequestContext);
		}

		public async Task DeleteAsync<T>(T item, IGraphRequestContext graphRequestContext) where T : class
		{
			await GetProvider<T>(graphRequestContext).DeleteAsync(item, graphRequestContext);
		}

		public async Task<IEnumerable<T>> QueryAsync<T>(string queryName, IEnumerable<QueryParameter> queryParameters,
			Dictionary<string, object> stepBagItems, IGraphRequestContext graphRequestContext) where T : class
		{
			var searchResult = new List<SearchResult>();
			var searchTypes = (Type[])stepBagItems[SearchConstants.QueryTypes];
			var queryParametersList = queryParameters.ToList();

			var enableAggregate = stepBagItems.ContainsKey(SearchConstants.EnableAggregate);

			foreach (var searchType in searchTypes)
			{
				var provider = _providers.Single(x => x.CanHandle(searchType.Name, graphRequestContext));

				var result = await provider.QueryAsync<SearchResultModel>(
					queryParametersList, searchType, enableAggregate, graphRequestContext);

				searchResult.Add(result);
			}

			return searchResult.Select(x => x as T).ToList();
		}

		public async Task DeleteAllAsync<T>(IGraphRequestContext graphRequestContext) where T : class
		{
			await GetProvider<T>(graphRequestContext).DeleteAllAsync<T>(graphRequestContext);
		}

		public async Task AddOrUpdateAsync<T>(T item, IGraphRequestContext graphRequestContext) where T : class
		{
			await GetProvider<T>(graphRequestContext).CreateOrUpdateAsync(item, graphRequestContext);
		}

		public async Task BatchAddOrUpdateAsync<T>(IEnumerable<T> items, IGraphRequestContext graphRequestContext) where T : class
		{
			await GetProvider<T>(graphRequestContext).BatchCreateOrUpdateAsync(items, graphRequestContext);
		}
	}
}
