using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Eklee.Azure.Functions.GraphQl.Repository.DocumentDb;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Eklee.Azure.Functions.GraphQl.Repository.TableStorage
{
	public class TableStorageInfo
	{
		public string Id { get; set; }
		public CloudTable Table { get; set; }
		public Func<IGraphRequestContext, bool> RequestContextSelector { get; set; }
		public string PartitionKeyMemberName { get; set; }
	}

	public class TableStorageClientProvider
	{
		private readonly CloudStorageAccount _storageAccount;
		private readonly ILogger _logger;
		private readonly IEnumerable<ITableStorageComparison> _tableStorageComparisons;
		private readonly string _url;
		private readonly List<TableStorageInfo> _tableStorageInfos = new List<TableStorageInfo>();

		public TableStorageClientProvider(CloudStorageAccount storageAccount, ILogger logger, IEnumerable<ITableStorageComparison> tableStorageComparisons)
		{
			_storageAccount = storageAccount;
			_logger = logger;
			_tableStorageComparisons = tableStorageComparisons;
			_url = storageAccount.TableEndpoint.ToString();
		}

		public bool ContainsTableEndpoint(CloudStorageAccount storageAccount)
		{
			return _url == storageAccount.TableEndpoint.ToString();
		}

		public async Task ConfigureTable(Dictionary<string, object> configurations, Type sourceType)
		{
			var prefix = configurations.GetStringValue(TableStorageConstants.Prefix, sourceType);
			if (prefix == null) prefix = "";

			var tableClient = _storageAccount.CreateCloudTableClient();
			var tableRef = tableClient.GetTableReference($"{prefix}{sourceType.Name}");
			await tableRef.CreateIfNotExistsAsync();

			var info = new TableStorageInfo
			{
				Id = sourceType.Name,
				RequestContextSelector = configurations.ContainsKey(DocumentDbConfigurationExtensions.GetKey(
					TableStorageConstants.RequestContextSelector, sourceType)) ?
					configurations.GetValue<Func<IGraphRequestContext, bool>>(TableStorageConstants.RequestContextSelector, sourceType) : null,
				Table = tableRef,
				PartitionKeyMemberName = configurations.GetValue<MemberExpression>(
					TableStorageConstants.PartitionMemberExpression, sourceType).Member.Name
			};

			_tableStorageInfos.Add(info);
		}

		public async Task BatchAddOrUpdateAsync<T>(IEnumerable<T> items, IGraphRequestContext graphRequestContext) where T : class
		{
			await InternalBatchAsync(items, graphRequestContext, false);
		}

		public async Task BatchAddAsync<T>(IEnumerable<T> items, IGraphRequestContext graphRequestContext) where T : class
		{
			await InternalBatchAsync(items, graphRequestContext, true);
		}

		private async Task InternalBatchAsync<T>(IEnumerable<T> items, IGraphRequestContext graphRequestContext, bool insert) where T : class
		{
			var info = Get<T>(graphRequestContext);

			Dictionary<string, TableBatchOperation> batchOperations = new Dictionary<string, TableBatchOperation>();

			items.ToList().ForEach(item =>
			{
				var partitionKey = item.GetMemberStringValue(info.PartitionKeyMemberName);
				if (!batchOperations.ContainsKey(partitionKey))
				{
					batchOperations.Add(partitionKey, new TableBatchOperation());
				}

				if (insert)
					batchOperations[partitionKey].Insert(Convert(item, partitionKey));
				else
					batchOperations[partitionKey].InsertOrMerge(Convert(item, partitionKey));
			});

			_logger.LogInformation($"Generated {batchOperations.Count} batch operations for {info.Id}.");

			foreach (var batchOperationKey in batchOperations.Keys)
			{
				_logger.LogInformation($"Processing batch operations for partition: {batchOperationKey}.");

				try
				{
					await info.Table.ExecuteBatchAsync(batchOperations[batchOperationKey]);
				}
				catch (StorageException ex)
				{
					if (ex.RequestInformation != null &&
						!string.IsNullOrEmpty(ex.RequestInformation.HttpStatusMessage))
					{
						_logger.LogError(ex.RequestInformation.HttpStatusMessage);
					}

					throw;
				}

			}
		}

		private DynamicTableEntity Convert<T>(T item, string partitionKey)
		{
			DynamicTableEntity dynamicTableEntity =
				new DynamicTableEntity(partitionKey, item.GetKey())
				{
					Properties = EntityPropertyConverter.Flatten(item, new OperationContext())
				};
			dynamicTableEntity.ETag = "*";
			return dynamicTableEntity;
		}

		private TableStorageInfo Get<T>(IGraphRequestContext graphRequestContext)
		{
			var info = InternalGet<T>(graphRequestContext);
			if (info == null) throw new ApplicationException("Unable to determine the correct RequestContextSelector to process request.");
			return info;
		}

		private TableStorageInfo InternalGet<T>(IGraphRequestContext graphRequestContext)
		{
			var name = typeof(T).Name;
			var infos = _tableStorageInfos.Where(x => x.Id == name).ToList();
			if (infos.Count == 1 && infos.Single().RequestContextSelector == null)
				return infos.Single();

			infos = infos.Where(x => x.RequestContextSelector != null && x.RequestContextSelector(graphRequestContext)).ToList();

			if (infos.Count == 1) return infos.Single();

			return null;
		}

		public async Task AddAsync<T>(T item, IGraphRequestContext graphRequestContext) where T : class
		{
			var info = Get<T>(graphRequestContext);
			var partitionKey = item.GetMemberStringValue(info.PartitionKeyMemberName);
			await info.Table.ExecuteAsync(TableOperation.Insert(Convert(item, partitionKey)));
		}

		public async Task AddOrUpdateAsync<T>(T item, IGraphRequestContext graphRequestContext) where T : class
		{
			var info = Get<T>(graphRequestContext);
			var partitionKey = item.GetMemberStringValue(info.PartitionKeyMemberName);
			await info.Table.ExecuteAsync(TableOperation.InsertOrMerge(Convert(item, partitionKey)));
		}

		public bool CanHandle<T>(IGraphRequestContext graphRequestContext)
		{
			return InternalGet<T>(graphRequestContext) != null;
		}

		public async Task UpdateAsync<T>(T item, IGraphRequestContext graphRequestContext) where T : class
		{
			var info = Get<T>(graphRequestContext);
			var partitionKey = item.GetMemberStringValue(info.PartitionKeyMemberName);
			await info.Table.ExecuteAsync(TableOperation.Replace(Convert(item, partitionKey)));
		}

		public async Task DeleteAsync<T>(T item, IGraphRequestContext graphRequestContext) where T : class
		{
			var info = Get<T>(graphRequestContext);
			var partitionKey = item.GetMemberStringValue(info.PartitionKeyMemberName);
			await info.Table.ExecuteAsync(TableOperation.Delete(Convert(item, partitionKey)));
		}

		public async Task DeleteAllAsync<T>(IGraphRequestContext graphRequestContext) where T : class
		{
			var info = Get<T>(graphRequestContext);

			while (true)
			{
				var entities = await info.Table.ExecuteQuerySegmentedAsync(new TableQuery(), null);

				if (entities.Results.Count == 0) break;

				Dictionary<string, TableBatchOperation> batchOperations = new Dictionary<string, TableBatchOperation>();

				entities.Results.ToList().ForEach(item =>
				{
					var partitionKey = item.PartitionKey;
					if (!batchOperations.ContainsKey(partitionKey))
					{
						batchOperations.Add(partitionKey, new TableBatchOperation());
					}
					batchOperations[partitionKey].Delete(item);
				});

				foreach (var batchOperationKey in batchOperations.Keys)
				{
					_logger.LogInformation($"Processing batch delete operations for partition: {batchOperationKey}.");

					await info.Table.ExecuteBatchAsync(batchOperations[batchOperationKey]);
				}
			}
		}

		public async Task<IEnumerable<T>> QueryAsync<T>(IEnumerable<QueryParameter> queryParameters,
			IGraphRequestContext graphRequestContext)
		{
			var info = Get<T>(graphRequestContext);

			var results = await info.Table.ExecuteQuerySegmentedAsync(GenerateTableQuery(queryParameters.ToList()), null);

			return results.Results.Select(x => EntityPropertyConverter.ConvertBack<T>(x.Properties, new OperationContext())).ToList();
		}

		private TableQuery GenerateTableQuery(List<QueryParameter> queryParameters)
		{
			string previousFilter = null;
			queryParameters.ForEach(queryParameter =>
			{
				if (previousFilter == null)
				{
					previousFilter = TranslateQueryParameter(queryParameter);
				}
				else
				{
					previousFilter = TableQuery.CombineFilters(previousFilter, TableOperators.And, TranslateQueryParameter(queryParameter));
				}
			});

			_logger.LogInformation($"Generated filter string in TableStorage provider: {previousFilter}");

			return new TableQuery { FilterString = previousFilter };
		}

		private string TranslateQueryParameter(QueryParameter queryParameter)
		{
			var comparison = _tableStorageComparisons.FirstOrDefault(x => x.CanHandle(queryParameter));

			if (comparison == null) throw new NotImplementedException($"Comparison {queryParameter.ContextValue.Comparison} is not implemented for type.");

			var statement = comparison.Generate();

			if (statement == null) throw new NotImplementedException($"Comparison {queryParameter.ContextValue.Comparison} is not implemented by {comparison.GetType().FullName}.");

			return statement;
		}
	}
}