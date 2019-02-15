﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
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
			var tableClient = _storageAccount.CreateCloudTableClient();
			var tableRef = tableClient.GetTableReference(sourceType.Name);
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

		public async Task BatchAddAsync<T>(IEnumerable<T> items, IGraphRequestContext graphRequestContext) where T : class
		{
			var info = Get<T>(graphRequestContext);

			TableBatchOperation batchOperation = new TableBatchOperation();

			items.ToList().ForEach(item => batchOperation.Insert(Convert(item, info.PartitionKeyMemberName)));

			await info.Table.ExecuteBatchAsync(batchOperation);
		}

		private DynamicTableEntity Convert<T>(T item, string partitionKeyMemberName)
		{
			DynamicTableEntity dynamicTableEntity =
				new DynamicTableEntity(item.GetMemberStringValue(partitionKeyMemberName), item.GetKey())
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
			var infos = _tableStorageInfos.Where(x => x.Id == typeof(T).Name).ToList();
			if (infos.Count == 1 && infos.Single().RequestContextSelector == null)
				return infos.Single();

			infos = infos.Where(x => x.RequestContextSelector != null && x.RequestContextSelector(graphRequestContext)).ToList();

			if (infos.Count == 1) return infos.Single();

			return null;
		}

		public async Task AddAsync<T>(T item, IGraphRequestContext graphRequestContext) where T : class
		{
			var info = Get<T>(graphRequestContext);

			await info.Table.ExecuteAsync(TableOperation.Insert(Convert(item, info.PartitionKeyMemberName)));
		}

		public bool CanHandle<T>(IGraphRequestContext graphRequestContext)
		{
			return InternalGet<T>(graphRequestContext) != null;
		}

		public async Task UpdateAsync<T>(T item, IGraphRequestContext graphRequestContext) where T : class
		{
			var info = Get<T>(graphRequestContext);

			await info.Table.ExecuteAsync(TableOperation.Replace(Convert(item, info.PartitionKeyMemberName)));
		}

		public async Task DeleteAsync<T>(T item, IGraphRequestContext graphRequestContext) where T : class
		{
			var info = Get<T>(graphRequestContext);

			await info.Table.ExecuteAsync(TableOperation.Delete(Convert(item, info.PartitionKeyMemberName)));
		}

		public async Task DeleteAllAsync<T>(IGraphRequestContext graphRequestContext) where T : class
		{
			var info = Get<T>(graphRequestContext);

			await info.Table.DeleteAsync();
			await info.Table.CreateAsync();
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