using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Eklee.Azure.Functions.GraphQl.Connections;
using Eklee.Azure.Functions.GraphQl.Repository.DocumentDb;
using Eklee.Azure.Functions.GraphQl.Repository.Search.Filters;
using FastMember;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Rest.Azure;
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
		private readonly ISearchFilterProvider _searchFilterProvider;
		private readonly ILogger _logger;
		private readonly List<SearchClientProviderInfo> _searchClientProviderInfos = new List<SearchClientProviderInfo>();

		public SearchClientProvider(ISearchFilterProvider searchFilterProvider,
			ILogger logger, string serviceName, string key)
		{
			_searchFilterProvider = searchFilterProvider;
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

			_searchServiceClient.Indexes.CreateOrUpdate(new Index(indexName, GetTypeFields(sourceType, true)));

			_searchClientProviderInfos.Add(new SearchClientProviderInfo
			{
				Id = id,
				RequestContextSelector = configurations.ContainsKey(DocumentDbConfigurationExtensions.GetKey(SearchConstants.RequestContextSelector, sourceType)) ?
					configurations.GetValue<Func<IGraphRequestContext, bool>>(SearchConstants.RequestContextSelector, sourceType) : null,
				SearchIndexClient = new SearchIndexClient(_searchServiceClient.SearchServiceName, indexName, _searchServiceClient.SearchCredentials)
			});
		}

		private List<Field> GetTypeFields(Type sourceType, bool isTop)
		{
			var source = TypeAccessor.Create(sourceType);

			return source.GetMembers().Select(x =>
			{
				if (x.GetAttribute(typeof(JsonIgnoreAttribute), false) != null)
				{
					return null;
				}

				if (x.IsList())
				{
					var argType = x.Type.GetGenericArguments()[0];
					return new Field(x.Name, DataType.Collection(DataType.Complex), GetTypeFields(argType, false));
				}

				DataType type;

				bool hasType = true;
				if (!TryGetDataType(x.Type, out type))
				{
					if (x.Type.IsClass)
					{
						return new Field(x.Name, DataType.Complex, GetTypeFields(x.Type, false));
					}

					hasType = false;
				}

				if (!hasType)
				{
					throw new ArgumentException($"Type of {x.Type.FullName} cannot be translated to Search Data-type.");
				}

				var isSearchable = x.Type == typeof(string);

				var isKey = isTop && x.GetAttribute(typeof(KeyAttribute), false) != null;
				var isFacetable = x.GetAttribute(typeof(IsFacetableAttribute), false) != null;
				var isFilterable = x.GetAttribute(typeof(IsFilterableAttribute), false) != null;

				var field = new Field(x.Name, type)
				{
					IsKey = isKey,
					IsSearchable = !isKey && isSearchable,
					IsFacetable = isFacetable,
					IsFilterable = isFilterable
				};

				return field;
			}).Where(field => field != null).ToList();
		}

		private bool TryGetDataType(Type type, out DataType dt)
		{
			if (type == typeof(string))
			{
				dt = DataType.String;
				return true;
			}

			if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
			{
				dt = DataType.DateTimeOffset;
				return true;
			}

			if (type == typeof(bool))
			{
				dt = DataType.Boolean;
				return true;
			}

			if (type == typeof(int))
			{
				dt = DataType.Int32;
				return true;
			}

			if (type == typeof(long))
			{
				dt = DataType.Int64;
				return true;
			}

			if (type == typeof(decimal))
			{
				dt = DataType.Double;
				return true;
			}

			if (type == typeof(double))
			{
				dt = DataType.Double;
				return true;
			}

			if (type == typeof(float))
			{
				dt = DataType.Double;
				return true;
			}

			if (type == typeof(Guid))
			{
				dt = DataType.String;
				return true;
			}

			return false;
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
			await client.Documents.IndexAsync(IndexBatch.New(list));
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

		public async Task<SearchResult> QueryAsync<T>(IEnumerable<QueryParameter> queryParameters, Type type, bool enableAggregate, IGraphRequestContext graphRequestContext) where T : class
		{
			var searchResult = new SearchResult
			{
				Values = new List<SearchResultModel>(),
				Aggregates = new List<SearchAggregateModel>()
			};

			var searchResultModels = new List<SearchResultModel>();

			var client = InternalGetInfo(type.Name, graphRequestContext).SearchIndexClient;

			var searchParameters = new SearchParameters();

			TypeAccessor accessor = TypeAccessor.Create(type);
			var members = accessor.GetMembers();

			if (enableAggregate)
			{
				searchParameters.Facets = members.Where(x => x.GetAttribute(typeof(IsFacetableAttribute), false) != null).Select(x => x.Name).ToList();
			}

			var searchTextParam = queryParameters.Single(x => x.MemberModel.Name == "searchtext");

			if (enableAggregate)
			{
				searchParameters.Filter = _searchFilterProvider.GenerateStringFilter(queryParameters, members);
			}

			var results = await client.Documents.SearchAsync((string)searchTextParam.ContextValue.GetFirstValue(), searchParameters);

			searchResult.AddValues(accessor, results);
			searchResult.AddFacets(results);

			return searchResult;
		}

		public async Task DeleteAllAsync<T>(IGraphRequestContext graphRequestContext) where T : class
		{
			var client = Get<T>(graphRequestContext);

			_logger.LogInformation($"Removing search index: {client.IndexName}");

			await _searchServiceClient.Indexes.DeleteAsync(client.IndexName);

			var fields = GetTypeFields(typeof(T), true);

			await _searchServiceClient.Indexes.CreateAsync(new Index(client.IndexName, fields));
		}
	}
}