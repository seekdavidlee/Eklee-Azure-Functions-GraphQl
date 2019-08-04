using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Eklee.Azure.Functions.GraphQl.Repository.DocumentDb;
using FastMember;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DataType = Microsoft.Azure.Search.Models.DataType;

namespace Eklee.Azure.Functions.GraphQl.Repository.Search
{
	public class SearchClientProviderInfo
	{
		public string Id { get; set; }
		public Func<IGraphRequestContext, bool> RequestContextSelector { get; set; }
		public SearchIndexClient SearchIndexClient { get; set; }
	}

	public class SearchClientProvider
	{
		private readonly SearchServiceClient _searchServiceClient;
		private readonly ILogger _logger;
		private readonly List<SearchClientProviderInfo> _searchClientProviderInfos = new List<SearchClientProviderInfo>();

		public SearchClientProvider(ILogger logger, string serviceName, string key)
		{
			_logger = logger;
			var searchCredentials = new SearchCredentials(key);
			_searchServiceClient = new SearchServiceClient(serviceName, searchCredentials);
		}

		public bool ContainsServiceName(string serviceName)
		{
			return _searchServiceClient.SearchServiceName == serviceName;
		}

		public void ConfigureSearchService(Dictionary<string, object> configurations, Type sourceType)
		{
			string id = sourceType.Name.ToLower();

			if (_searchClientProviderInfos.Any(x => x.Id == id)) return;

			var prefix = configurations.GetStringValue(SearchConstants.Prefix, sourceType) ?? "";
			string indexName = prefix + id;

			_searchServiceClient.Indexes.CreateOrUpdate(new Index(indexName, GetTypeFields(sourceType)));

			_searchClientProviderInfos.Add(new SearchClientProviderInfo
			{
				Id = id,
				RequestContextSelector = configurations.ContainsKey(DocumentDbConfigurationExtensions.GetKey(SearchConstants.RequestContextSelector, sourceType)) ?
					configurations.GetValue<Func<IGraphRequestContext, bool>>(SearchConstants.RequestContextSelector, sourceType) : null,
				SearchIndexClient = new SearchIndexClient(_searchServiceClient.SearchServiceName, indexName, _searchServiceClient.SearchCredentials)
			});
		}

		private List<Field> GetTypeFields(Type sourceType)
		{
			var source = TypeAccessor.Create(sourceType);

			return source.GetMembers().Select(x =>
			{
				if (x.GetAttribute(typeof(JsonIgnoreAttribute), false) != null)
				{
					return null;
				}

				DataType type = GetDataType(x.Type);
				var isSearchable = x.Type == typeof(string);
				var isKey = x.GetAttribute(typeof(KeyAttribute), false) != null;
				var isFacetable = x.GetAttribute(typeof(IsFacetableAttribute), false) != null;

				var field = new Field(x.Name, type)
				{
					IsKey = isKey,
					IsSearchable = !isKey && isSearchable,
					IsFacetable = isFacetable
				};

				return field;
			}).Where(field => field != null).ToList();
		}

		private DataType GetDataType(Type type)
		{
			if (type == typeof(string))
				return DataType.String;

			if (type == typeof(DateTime))
				return DataType.DateTimeOffset;

			if (type == typeof(bool))
				return DataType.Boolean;

			if (type == typeof(int))
				return DataType.Int32;

			if (type == typeof(long))
				return DataType.Int64;

			if (type == typeof(decimal))
				return DataType.Double;

			if (type == typeof(double))
				return DataType.Double;

			if (type == typeof(float))
				return DataType.Double;

			if (type == typeof(Guid))
				return DataType.String;

			throw new ArgumentException($"Type of {type.FullName} cannot be translated to Search Data-type.");
		}

		public bool CanHandle<T>(IGraphRequestContext graphRequestContext)
		{
			return GetInfo<T>(graphRequestContext) != null;
		}

		public bool CanHandle(string typeName, IGraphRequestContext graphRequestContext)
		{
			return InternalGetInfo(typeName.ToLower(), graphRequestContext) != null;
		}

		private SearchIndexClient Get<T>(IGraphRequestContext graphRequestContext)
		{
			var info = GetInfo<T>(graphRequestContext);

			if (info == null) throw new ApplicationException("Unable to determine the correct RequestContextSelector to process request.");

			return info.SearchIndexClient;
		}

		private SearchClientProviderInfo GetInfo<T>(IGraphRequestContext graphRequestContext)
		{
			var indexName = typeof(T).Name;
			return InternalGetInfo(indexName, graphRequestContext);
		}

		private SearchClientProviderInfo InternalGetInfo(string indexName, IGraphRequestContext graphRequestContext)
		{
			indexName = indexName.ToLower();
			var infos = _searchClientProviderInfos.Where(x => x.Id == indexName).ToList();

			if (infos.Count == 1 && infos.Single().RequestContextSelector == null) return infos.Single();

			infos = infos.Where(x => x.RequestContextSelector != null && x.RequestContextSelector(graphRequestContext)).ToList();
			if (infos.Count == 1) return infos.Single();

			return null;
		}

		public async Task BatchCreateAsync<T>(IEnumerable<T> items, IGraphRequestContext graphRequestContext) where T : class
		{
			var client = Get<T>(graphRequestContext);
			var list = items.Select(IndexAction.Upload).ToList();
			await client.Documents.IndexAsync(new IndexBatch<T>(list));
		}

		public Task BatchCreateOrUpdateAsync<T>(IEnumerable<T> items, IGraphRequestContext graphRequestContext) where T : class
		{
			throw new NotImplementedException();
		}

		public Task CreateOrUpdateAsync<T>(T item, IGraphRequestContext graphRequestContext) where T : class
		{
			throw new NotImplementedException();
		}

		public async Task CreateAsync<T>(T item, IGraphRequestContext graphRequestContext) where T : class
		{
			var client = Get<T>(graphRequestContext);
			var idx = IndexAction.Upload(item);
			await client.Documents.IndexAsync(new IndexBatch<T>(
				new List<IndexAction<T>> { idx }));
		}

		public async Task UpdateAsync<T>(T item, IGraphRequestContext graphRequestContext) where T : class
		{
			var client = Get<T>(graphRequestContext);
			var idx = IndexAction.Merge(item);
			await client.Documents.IndexAsync(new IndexBatch<T>(
				new List<IndexAction<T>> { idx }));

		}

		public async Task DeleteAsync<T>(T item, IGraphRequestContext graphRequestContext) where T : class
		{
			var client = Get<T>(graphRequestContext);
			var idx = IndexAction.Delete(item);
			await client.Documents.IndexAsync(new IndexBatch<T>(
				new List<IndexAction<T>> { idx }));
		}

		public async Task<IEnumerable<T>> QueryAsync<T>(IEnumerable<QueryParameter> queryParameters, Type type, IGraphRequestContext graphRequestContext) where T : class
		{
			var searchResultModels = new List<SearchResultModel>();

			var client = InternalGetInfo(type.Name, graphRequestContext).SearchIndexClient;

			var searchParameters = new SearchParameters();

			var searchTextParam = queryParameters.Single(x => x.MemberModel.Name == "searchtext");

			var results = await client.Documents.SearchAsync((string)searchTextParam.ContextValue.GetFirstValue(), searchParameters);

			TypeAccessor accessor = TypeAccessor.Create(type);
			var members = accessor.GetMembers();
			results.Results.ToList().ForEach(r =>
			{
				var item = accessor.CreateNew();

				r.Document.ToList().ForEach(d =>
				{
					var field = members.Single(x => x.Name == d.Key);

					object value = d.Value;

					if (field.Type == typeof(Guid) && value is string strValue)
					{
						value = Guid.Parse(strValue);
					}

					if (field.Type == typeof(decimal) && value is double dobValue)
					{
						value = Convert.ToDecimal(dobValue);
					}

					if (field.Type == typeof(DateTime) && value is DateTimeOffset dtmValue)
					{
						value = dtmValue.DateTime;
					}

					if (field.Type == typeof(int))
					{
						value = Convert.ToInt32(value);
					}

					if (field.Type == typeof(long))
					{
						value = Convert.ToInt64(value);
					}

					accessor[item, d.Key] = value;
				});

				var searchResultModel = new SearchResultModel
				{
					Score = r.Score,
					Value = item
				};
				searchResultModels.Add(searchResultModel);
			});

			return searchResultModels.Select(x => x as T).ToList();
		}

		public async Task DeleteAllAsync<T>(IGraphRequestContext graphRequestContext) where T : class
		{
			var client = Get<T>(graphRequestContext);

			_logger.LogInformation($"Removing search index: {client.IndexName}");

			await _searchServiceClient.Indexes.DeleteAsync(client.IndexName);

			var fields = GetTypeFields(typeof(T));

			await _searchServiceClient.Indexes.CreateAsync(new Index(client.IndexName, fields));
		}
	}
}