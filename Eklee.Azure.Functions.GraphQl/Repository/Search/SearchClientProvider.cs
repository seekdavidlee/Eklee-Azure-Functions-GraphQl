using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Eklee.Azure.Functions.GraphQl.Connections;
using Eklee.Azure.Functions.GraphQl.Repository.DocumentDb;
using Eklee.Azure.Functions.GraphQl.Repository.Search.Filters;
using FastMember;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Eklee.Azure.Functions.GraphQl.Repository.Search
{
	public class SearchClientProviderInfo
	{
		public string Id { get; set; }
		public Func<IGraphRequestContext, bool> RequestContextSelector { get; set; }
		public SearchClient SearchIndexClient { get; set; }
	}

	public class IsFacetableAttribute : Attribute
	{

	}

	public class IsFilterableAttribute : Attribute
	{

	}

	public class SearchClientProvider
	{
		private readonly SearchIndexClient _searchServiceClient;
		private readonly ISearchFilterProvider _searchFilterProvider;
		private readonly ILogger _logger;
		private readonly List<SearchClientProviderInfo> _searchClientProviderInfos = new List<SearchClientProviderInfo>();
		private readonly string _serviceName;
		private readonly AzureKeyCredential _azureKeyCredential;

		public SearchClientProvider(ISearchFilterProvider searchFilterProvider,
			ILogger logger, string serviceName, string key)
		{
			_serviceName = serviceName;
			_searchFilterProvider = searchFilterProvider;
			_logger = logger;
			_azureKeyCredential = new AzureKeyCredential(key);
			_searchServiceClient = new SearchIndexClient(_serviceName.GetSearchServiceUri(), _azureKeyCredential);

		}

		public bool ContainsServiceName(string serviceName)
		{
			return _serviceName == serviceName;
		}

		public void ConfigureSearchService(Dictionary<string, object> configurations, Type sourceType)
		{
			string id = sourceType.Name.ToLower();

			if (_searchClientProviderInfos.Any(x => x.Id == id)) return;

			var prefix = configurations.GetStringValue(SearchConstants.Prefix, sourceType) ?? "";
			string indexName = prefix + id;

			_searchServiceClient.CreateOrUpdateIndex(new SearchIndex(indexName, GetTypeFields(sourceType, true)));

			_searchClientProviderInfos.Add(new SearchClientProviderInfo
			{
				Id = id,
				RequestContextSelector = configurations.ContainsKey(DocumentDbConfigurationExtensions.GetKey(SearchConstants.RequestContextSelector, sourceType)) ?
					configurations.GetValue<Func<IGraphRequestContext, bool>>(SearchConstants.RequestContextSelector, sourceType) : null,
				SearchIndexClient = new SearchClient(_serviceName.GetSearchServiceUri(), indexName, _azureKeyCredential)
			});
		}

		private List<SearchField> GetTypeFields(Type sourceType, bool isTop)
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

					var complexCollection = new SearchField(x.Name, SearchFieldDataType.Collection(SearchFieldDataType.Complex));
					GetTypeFields(argType, false).ForEach(complexCollection.Fields.Add);
					return complexCollection;
				}

				SearchFieldDataType type;

				bool hasType = true;
				if (!TryGetDataType(x.Type, out type))
				{
					if (x.Type.IsClass)
					{
						var complex = new SearchField(x.Name, SearchFieldDataType.Complex);
						GetTypeFields(x.Type, false).ForEach(complex.Fields.Add);
						return complex;
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

				var field = new SearchField(x.Name, type)
				{
					IsKey = isKey,
					IsSearchable = !isKey && isSearchable,
					IsFacetable = isFacetable,
					IsFilterable = isFilterable
				};

				return field;
			}).Where(field => field != null).ToList();
		}

		private bool TryGetDataType(Type type, out SearchFieldDataType dt)
		{
			if (type == typeof(string))
			{
				dt = SearchFieldDataType.String;
				return true;
			}

			if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
			{
				dt = SearchFieldDataType.DateTimeOffset;
				return true;
			}

			if (type == typeof(bool))
			{
				dt = SearchFieldDataType.Boolean;
				return true;
			}

			if (type == typeof(int))
			{
				dt = SearchFieldDataType.Int32;
				return true;
			}

			if (type == typeof(long))
			{
				dt = SearchFieldDataType.Int64;
				return true;
			}

			if (type == typeof(decimal))
			{
				dt = SearchFieldDataType.Double;
				return true;
			}

			if (type == typeof(double))
			{
				dt = SearchFieldDataType.Double;
				return true;
			}

			if (type == typeof(float))
			{
				dt = SearchFieldDataType.Double;
				return true;
			}

			if (type == typeof(Guid))
			{
				dt = SearchFieldDataType.String;
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

		private SearchClient Get<T>(IGraphRequestContext graphRequestContext)
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

			var list = items.Select(IndexDocumentsAction.Upload).ToArray();
			await client.IndexDocumentsAsync(IndexDocumentsBatch.Create(list));
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
			await BatchCreateAsync(new List<T> { item }, graphRequestContext);
		}

		public async Task UpdateAsync<T>(T item, IGraphRequestContext graphRequestContext) where T : class
		{
			var client = Get<T>(graphRequestContext);

			var batch = IndexDocumentsBatch.Create(IndexDocumentsAction.Merge(item));
			await client.IndexDocumentsAsync(batch);
		}

		public async Task DeleteAsync<T>(T item, IGraphRequestContext graphRequestContext) where T : class
		{
			var client = Get<T>(graphRequestContext);

			var batch = IndexDocumentsBatch.Create(IndexDocumentsAction.Delete(item));
			await client.IndexDocumentsAsync(batch);
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

			var searchOptions = new SearchOptions();

			TypeAccessor accessor = TypeAccessor.Create(type);
			var members = accessor.GetMembers();

			if (enableAggregate)
			{
				members.Where(x => x.GetAttribute(typeof(IsFacetableAttribute), false) != null).Select(x => x.Name)
				   .ToList()
				   .ForEach(searchOptions.Facets.Add);
			}

			var searchTextParam = queryParameters.Single(x => x.MemberModel.Name == "searchtext");

			if (enableAggregate)
			{
				searchOptions.Filter = _searchFilterProvider.GenerateStringFilter(queryParameters, members);
			}

			var results = await client.SearchAsync<SearchDocument>((string)searchTextParam.ContextValue.GetFirstValue(), searchOptions);

			searchResult.AddValues(accessor, results);
			searchResult.AddFacets(results);

			return searchResult;
		}

		public async Task DeleteAllAsync<T>(IGraphRequestContext graphRequestContext) where T : class
		{
			var client = Get<T>(graphRequestContext);

			_logger.LogInformation($"Removing search index: {client.IndexName}");

			await _searchServiceClient.DeleteIndexAsync(client.IndexName);

			var fields = GetTypeFields(typeof(T), true);

			await _searchServiceClient.CreateIndexAsync(new SearchIndex(client.IndexName, fields));
		}
	}
}