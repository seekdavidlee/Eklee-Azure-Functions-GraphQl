using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Eklee.Azure.Functions.GraphQl.Repository.DocumentDb
{
	public class DocumentDbRepository : IGraphQlRepository
	{
		private readonly ILogger _logger;
		private readonly IEnumerable<IDocumentDbComparison> _documentDbComparisons;
		private readonly Dictionary<string, DocumentClientProvider> _providers = new Dictionary<string, DocumentClientProvider>();
		private readonly Dictionary<string, DocumentClientProvider> _typedProviders = new Dictionary<string, DocumentClientProvider>();

		public DocumentDbRepository(ILogger logger, IEnumerable<IDocumentDbComparison> documentDbComparisons)
		{
			_logger = logger;
			_documentDbComparisons = documentDbComparisons;
		}

		public void Configure(Type sourceType, Dictionary<string, object> configurations)
		{
			var url = configurations.GetStringValue(DocumentDbConstants.Url, sourceType);

			DocumentClientProvider provider;

			if (_providers.ContainsKey(url))
			{
				provider = _providers[url];
			}
			else
			{
				provider = new DocumentClientProvider(url, configurations.GetStringValue(DocumentDbConstants.Key, sourceType), _documentDbComparisons, _logger);
				_providers.Add(url, provider);
			}

			try
			{
				provider.ConfigureDatabaseAndCollection(configurations, sourceType).GetAwaiter().GetResult();
			}
			catch (Exception e)
			{
				_logger.LogError(e, $"An error has occured while creating a DocumentDb repository instance for {sourceType.FullName}.");
				throw;
			}

			_typedProviders[sourceType.Name] = provider;
		}

		public async Task BatchAddAsync<T>(IEnumerable<T> items) where T : class
		{
			var provider = GetProvider<T>();

			foreach (var item in items)
			{
				await provider.CreateAsync(item);
			}
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

		private DocumentClientProvider GetProvider<T>()
		{
			return _typedProviders[typeof(T).Name];
		}

		public async Task<IEnumerable<T>> QueryAsync<T>(string queryName, IEnumerable<QueryParameter> queryParameters, Dictionary<string, object> stepBagItems) where T : class
		{
			return await GetProvider<T>().QueryAsync<T>(queryParameters);
		}

		public async Task DeleteAllAsync<T>() where T : class
		{
			await GetProvider<T>().DeleteAllAsync<T>();
		}
	}
}
