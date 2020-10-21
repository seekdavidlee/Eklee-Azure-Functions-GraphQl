using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Eklee.Azure.Functions.GraphQl.Repository.DocumentDb
{
	public class DocumentDbRepository : IGraphQlRepository
	{
		private readonly ILogger _logger;
		private readonly IEnumerable<IDocumentDbComparison> _documentDbComparisons;
		private readonly List<DocumentClientProvider> _providers = new List<DocumentClientProvider>();

		public DocumentDbRepository(ILogger logger, IEnumerable<IDocumentDbComparison> documentDbComparisons)
		{
			_logger = logger;
			_documentDbComparisons = documentDbComparisons;
		}

		public void Configure(Type sourceType, Dictionary<string, object> configurations)
		{
			var url = configurations.GetStringValue(DocumentDbConstants.Url, sourceType);

			_logger.LogInformation($"Configuring document client provider with url: {url}");

			DocumentClientProvider provider = _providers.SingleOrDefault(p => p.ContainsUrl(url));

			if (provider == null)
			{
				provider = new DocumentClientProvider(url, configurations.GetStringValue(DocumentDbConstants.Key, sourceType), _documentDbComparisons, _logger);
				_providers.Add(provider);
			}

			try
			{
				provider.ConfigureDatabaseAndCollection(configurations, sourceType).GetAwaiter().GetResult();
				_logger.LogInformation($"Document client provider configured.");
			}
			catch (Exception e)
			{
				_logger.LogError(e, $"An error has occured while creating a DocumentDb repository instance for {sourceType.FullName}.");
				throw;
			}
		}

		public async Task BatchAddAsync<T>(IEnumerable<T> items, IGraphRequestContext graphRequestContext) where T : class
		{
			var provider = GetProvider<T>(graphRequestContext);

			foreach (var item in items)
			{
				await provider.CreateAsync(item, graphRequestContext);
			}
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

		private DocumentClientProvider GetProvider<T>(IGraphRequestContext graphRequestContext)
		{
			return _providers.Single(x => x.CanHandle<T>(graphRequestContext));
		}

		public async Task<IEnumerable<T>> QueryAsync<T>(string queryName, IEnumerable<QueryParameter> queryParameters, Dictionary<string, object> stepBagItems, IGraphRequestContext graphRequestContext) where T : class
		{
			return await GetProvider<T>(graphRequestContext).QueryAsync<T>(queryParameters, graphRequestContext);
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
			var provider = GetProvider<T>(graphRequestContext);

			foreach (var item in items)
			{
				await provider.CreateOrUpdateAsync(item, graphRequestContext);
			}
		}
	}
}
