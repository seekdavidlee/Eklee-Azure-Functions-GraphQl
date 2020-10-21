using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eklee.Azure.Functions.GraphQl.Repository.DocumentDb;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;

namespace Eklee.Azure.Functions.GraphQl.Repository.TableStorage
{
	public class TableStorageRepository : IGraphQlRepository
	{
		private readonly ILogger _logger;
		private readonly IEnumerable<ITableStorageComparison> _tableStorageComparisons;
		private readonly List<TableStorageClientProvider> _providers = new List<TableStorageClientProvider>();

		public TableStorageRepository(ILogger logger, IEnumerable<ITableStorageComparison> tableStorageComparisons)
		{
			_logger = logger;
			_tableStorageComparisons = tableStorageComparisons;
		}

		public void Configure(Type sourceType, Dictionary<string, object> configurations)
		{
			CloudStorageAccount storageAccount =
				CloudStorageAccount.Parse(configurations.GetStringValue(TableStorageConstants.ConnectionString,
					sourceType));

			var provider = _providers.SingleOrDefault(p => p.ContainsTableEndpoint(storageAccount));

			if (provider == null)
			{
				provider = new TableStorageClientProvider(storageAccount, _logger, _tableStorageComparisons);
				_providers.Add(provider);
			}

			try
			{
				_logger.LogInformation("Configuring table storage provider");
				provider.ConfigureTable(configurations, sourceType).GetAwaiter().GetResult();
				_logger.LogInformation("Table storage provider configured.");
			}
			catch (Exception e)
			{
				_logger.LogError(e, $"An error has occured while creating a TableStorage repository instance for {sourceType.FullName}.");
				throw;
			}
		}

		public async Task BatchAddAsync<T>(IEnumerable<T> items, IGraphRequestContext graphRequestContext) where T : class
		{
			await GetProvider<T>(graphRequestContext).BatchAddAsync(items, graphRequestContext);
		}

		public async Task AddAsync<T>(T item, IGraphRequestContext graphRequestContext) where T : class
		{
			await GetProvider<T>(graphRequestContext).AddAsync(item, graphRequestContext);
		}

		public async Task UpdateAsync<T>(T item, IGraphRequestContext graphRequestContext) where T : class
		{
			await GetProvider<T>(graphRequestContext).UpdateAsync(item, graphRequestContext);
		}

		public async Task DeleteAsync<T>(T item, IGraphRequestContext graphRequestContext) where T : class
		{
			await GetProvider<T>(graphRequestContext).DeleteAsync(item, graphRequestContext);
		}

		public async Task<IEnumerable<T>> QueryAsync<T>(string queryName, IEnumerable<QueryParameter> queryParameters, Dictionary<string, object> stepBagItems,
			IGraphRequestContext graphRequestContext) where T : class
		{
			return await GetProvider<T>(graphRequestContext).QueryAsync<T>(queryParameters, graphRequestContext);
		}

		public async Task DeleteAllAsync<T>(IGraphRequestContext graphRequestContext) where T : class
		{
			await GetProvider<T>(graphRequestContext).DeleteAllAsync<T>(graphRequestContext);
		}

		private TableStorageClientProvider GetProvider<T>(IGraphRequestContext graphRequestContext)
		{
			return _providers.Single(x => x.CanHandle<T>(graphRequestContext));
		}

		public async Task AddOrUpdateAsync<T>(T item, IGraphRequestContext graphRequestContext) where T : class
		{
			await GetProvider<T>(graphRequestContext).AddOrUpdateAsync(item, graphRequestContext);
		}

		public async Task BatchAddOrUpdateAsync<T>(IEnumerable<T> items, IGraphRequestContext graphRequestContext) where T : class
		{
			await GetProvider<T>(graphRequestContext).BatchAddOrUpdateAsync(items, graphRequestContext);
		}
	}
}
