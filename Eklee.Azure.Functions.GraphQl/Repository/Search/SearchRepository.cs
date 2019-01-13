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
		private readonly Dictionary<string, SearchClientProvider> _providers = new Dictionary<string, SearchClientProvider>();
		private readonly Dictionary<string, SearchClientProvider> _typedProviders = new Dictionary<string, SearchClientProvider>();
		private readonly Dictionary<string, Dictionary<string, Type[]>> _searchTypes = new Dictionary<string, Dictionary<string, Type[]>>();

		public SearchRepository(ILogger logger)
		{
			_logger = logger;
		}

		public void Configure(Type sourceType, Dictionary<string, object> configurations)
		{
			var serviceName = configurations.GetStringValue(SearchConstants.ServiceName, sourceType);

			SearchClientProvider searchClientProvider;

			if (_providers.ContainsKey(serviceName))
			{
				searchClientProvider = _providers[serviceName];
			}
			else
			{
				searchClientProvider = new SearchClientProvider(_logger, serviceName, configurations.GetStringValue(SearchConstants.ApiKey, sourceType));
				_providers.Add(serviceName, searchClientProvider);
			}

			searchClientProvider.ConfigureSearchService(configurations, sourceType);

			_typedProviders[sourceType.Name] = searchClientProvider;

			// ReSharper disable once AssignNullToNotNullAttribute
			if (!_searchTypes.ContainsKey(sourceType.FullName))
			{
				_searchTypes.Add(sourceType.FullName, new Dictionary<string, Type[]>());
			}
		}

		private SearchClientProvider GetProvider<T>() where T : class
		{
			return GetProvider(typeof(T).Name);
		}

		private SearchClientProvider GetProvider(string typeName)
		{
			return _typedProviders[typeName];
		}

		public async Task BatchAddAsync<T>(IEnumerable<T> items) where T : class
		{
			var provider = GetProvider<T>();
			await provider.BatchCreateAsync(items);
		}

		public async Task AddAsync<T>(T item) where T : class
		{
			await GetProvider<T>().CreateAsync(item);
		}

		public async Task UpdateAsync<T>(T item) where T : class
		{
			await GetProvider<T>().UpdateAsync(item);
		}

		public async Task DeleteAsync<T>(T item) where T : class
		{
			await GetProvider<T>().DeleteAsync(item);
		}

		public async Task<IEnumerable<T>> QueryAsync<T>(string queryName, IEnumerable<QueryParameter> queryParameters, Dictionary<string, object> stepBagItems) where T : class
		{
			List<SearchResultModel> searchResultModels = new List<SearchResultModel>();
			var searchTypes = (Type[])stepBagItems[SearchConstants.QueryTypes];
			var queryParametersList = queryParameters.ToList();

			foreach (var searchType in searchTypes)
			{
				var results = await GetProvider(searchType.Name)
					.QueryAsync<SearchResultModel>(queryParametersList, searchType);

				searchResultModels.AddRange(results);
			}

			var res = searchResultModels.Select(x => x as T).ToList();
			return res;
		}

		public async Task DeleteAllAsync<T>() where T : class
		{
			await GetProvider<T>().DeleteAllAsync<T>();
		}
	}
}
