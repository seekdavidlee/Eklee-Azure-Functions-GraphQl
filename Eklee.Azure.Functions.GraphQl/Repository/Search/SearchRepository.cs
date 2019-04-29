using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eklee.Azure.Functions.GraphQl.Repository.DocumentDb;
using Microsoft.Extensions.Logging;

namespace Eklee.Azure.Functions.GraphQl.Repository.Search
{
	public class SearchRepository : IGraphQlRepository
	{
		private readonly ILogger _logger;
		private readonly List<SearchClientProvider> _providers = new List<SearchClientProvider>();

		public SearchRepository(ILogger logger)
		{
			_logger = logger;
		}

		public void Configure(Type sourceType, Dictionary<string, object> configurations)
		{
			var serviceName = configurations.GetStringValue(SearchConstants.ServiceName, sourceType);

			SearchClientProvider searchClientProvider = _providers.SingleOrDefault(p => p.ContainsServiceName(serviceName));

			if (searchClientProvider == null)
			{
				searchClientProvider = new SearchClientProvider(_logger, serviceName, configurations.GetStringValue(SearchConstants.ApiKey, sourceType));
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
			List<SearchResultModel> searchResultModels = new List<SearchResultModel>();
			var searchTypes = (Type[])stepBagItems[SearchConstants.QueryTypes];
			var queryParametersList = queryParameters.ToList();

			foreach (var searchType in searchTypes)
			{
				var provider = _providers.Single(x => x.CanHandle(searchType.Name, graphRequestContext));
				var results = await provider.QueryAsync<SearchResultModel>(queryParametersList, searchType, graphRequestContext);
				searchResultModels.AddRange(results);
			}

			var res = searchResultModels.Select(x => x as T).ToList();
			return res;
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
