using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FastMember;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Logging;
using DataType = Microsoft.Azure.Search.Models.DataType;

namespace Eklee.Azure.Functions.GraphQl.Repository.Search
{
	public class SearchClientProvider
	{
		private readonly SearchServiceClient _searchServiceClient;
		private readonly ILogger _logger;
		private readonly string _serviceName;
		private readonly SearchCredentials _searchCredentials;
		private readonly Dictionary<string, SearchIndexClient> _searchIndexClients = new Dictionary<string, SearchIndexClient>();
		public SearchClientProvider(ILogger logger, string serviceName, string key)
		{
			_logger = logger;
			_serviceName = serviceName;
			_searchCredentials = new SearchCredentials(key);
			_searchServiceClient = new SearchServiceClient(_serviceName, _searchCredentials);
		}

		public void ConfigureSearchService(Dictionary<string, object> configurations, Type sourceType)
		{
			var fields = GetTypeFields(sourceType);

			_searchServiceClient.Indexes.CreateOrUpdate(new Index(sourceType.Name.ToLower(), fields));
		}

		private List<Field> GetTypeFields(Type sourceType)
		{
			var source = TypeAccessor.Create(sourceType);

			return source.GetMembers().Select(x =>
			{
				DataType type = null;
				var isSearchable = false;

				if (x.Type == typeof(string))
				{
					type = DataType.String;
					isSearchable = true;
				}

				if (x.Type == typeof(DateTime))
					type = DataType.DateTimeOffset;

				if (x.Type == typeof(bool))
					type = DataType.Boolean;

				if (x.Type == typeof(int))
					type = DataType.Int32;

				if (x.Type == typeof(long))
					type = DataType.Int64;

				if (x.Type == typeof(decimal))
					type = DataType.Double;

				if (x.Type == typeof(double))
					type = DataType.Double;

				if (x.Type == typeof(float))
					type = DataType.Double;

				if (x.Type == typeof(Guid))
					type = DataType.String;

				if (type == null) throw new ArgumentException($"Type of {x.Type.FullName} cannot be translated to Search Data-type.");

				var isKey = x.GetAttribute(typeof(KeyAttribute), false) != null;

				var field = new Field(x.Name, type)
				{
					IsKey = isKey,
					IsSearchable = !isKey && isSearchable
				};

				return field;
			}).ToList();
		}

		private SearchIndexClient GetSearchIndexClient<T>()
		{
			var indexName = typeof(T).Name.ToLower();
			return GetSearchIndexClient(indexName);
		}

		private SearchIndexClient GetSearchIndexClient(string indexName)
		{
			SearchIndexClient client;
			if (_searchIndexClients.ContainsKey(indexName))
			{
				client = _searchIndexClients[indexName];
			}
			else
			{
				client = new SearchIndexClient(_serviceName, indexName, _searchCredentials);
				_searchIndexClients.Add(indexName, client);
			}

			return client;
		}

		public async Task BatchCreateAsync<T>(IEnumerable<T> items) where T : class
		{
			var client = GetSearchIndexClient<T>();
			var list = items.Select(IndexAction.Upload).ToList();
			await client.Documents.IndexAsync(new IndexBatch<T>(list));
		}

		public async Task CreateAsync<T>(T item) where T : class
		{
			var client = GetSearchIndexClient<T>();
			var idx = IndexAction.Upload(item);
			await client.Documents.IndexAsync(new IndexBatch<T>(
				new List<IndexAction<T>> { idx }));
		}

		public async Task UpdateAsync<T>(T item) where T : class
		{
			var client = GetSearchIndexClient<T>();
			var idx = IndexAction.Merge(item);
			await client.Documents.IndexAsync(new IndexBatch<T>(
				new List<IndexAction<T>> { idx }));

		}

		public async Task DeleteAsync<T>(T item) where T : class
		{
			var client = GetSearchIndexClient<T>();
			var idx = IndexAction.Delete(item);
			await client.Documents.IndexAsync(new IndexBatch<T>(
				new List<IndexAction<T>> { idx }));
		}

		public async Task<IEnumerable<T>> QueryAsync<T>(IEnumerable<QueryParameter> queryParameters, Type type) where T : class
		{
			var searchResultModels = new List<SearchResultModel>();

			var client = GetSearchIndexClient(type.Name.ToLower());

			var searchParameters = new SearchParameters();

			var searchTextParam = queryParameters.Single(x => x.MemberModel.Name == "searchtext");

			var results = await client.Documents.SearchAsync((string)searchTextParam.ContextValue.Value, searchParameters);

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

		public async Task DeleteAllAsync<T>() where T : class
		{
			var indexName = typeof(T).Name.ToLower();

			_logger.LogInformation($"Removing search index: {indexName}");

			await _searchServiceClient.Indexes.DeleteAsync(indexName);

			var fields = GetTypeFields(typeof(T));

			await _searchServiceClient.Indexes.CreateAsync(new Index(indexName, fields));
		}
	}
}