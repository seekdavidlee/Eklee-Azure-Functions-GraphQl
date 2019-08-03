using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FastMember;
using GraphQL;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Eklee.Azure.Functions.GraphQl.Repository.DocumentDb
{
	public class DocumentTypeInfo
	{
		public PartitionKeyDefinition Partition { get; set; }
		public int RequestUnit { get; set; }
		public string Id { get; set; }
		public MemberExpression Expression { get; set; }
		public Func<IGraphRequestContext, bool> RequestContextSelector { get; set; }
		public string DatabaseId { get; set; }
	}

	public class DocumentClientProvider
	{
		private readonly string _url;
		private readonly IEnumerable<IDocumentDbComparison> _documentDbComparisons;
		private readonly ILogger _logger;
		private const int DefaultRequestUnits = 400;
		private readonly DocumentClient _documentClient;
		private readonly List<DocumentTypeInfo> _documentTypeInfos = new List<DocumentTypeInfo>();

		public DocumentClientProvider(string url, string key, IEnumerable<IDocumentDbComparison> documentDbComparisons, ILogger logger)
		{
			_url = url;
			_documentDbComparisons = documentDbComparisons;
			_logger = logger;
			_documentClient = new DocumentClient(new Uri(url), key);
		}

		public bool ContainsUrl(string url)
		{
			return _url == url;
		}

		public async Task ConfigureDatabaseAndCollection(
			Dictionary<string, object> configurations, Type sourceType)
		{
			int requestUnit = DefaultRequestUnits;

			var databaseId = configurations.GetStringValue(DocumentDbConstants.Database, sourceType);

			if (configurations.ContainsKey(DocumentDbConfigurationExtensions.GetKey(
				DocumentDbConstants.RequestUnit, sourceType)))
			{
				int.TryParse(configurations.GetStringValue(
					DocumentDbConstants.RequestUnit, sourceType), out requestUnit);
			}

			if (_documentTypeInfos.Count(x => x.DatabaseId == databaseId) == 0)
			{
				await _documentClient.CreateDatabaseIfNotExistsWithRetryAsync(databaseId, requestUnit);
			}

			var memberExpression = configurations.GetValue<MemberExpression>(
				DocumentDbConstants.PartitionMemberExpression, sourceType);

			var documentTypeInfo = new DocumentTypeInfo
			{
				Id = sourceType.Name,
				DatabaseId = databaseId,
				Partition = new PartitionKeyDefinition
				{
					Paths = new Collection<string> { $"/{memberExpression.Member.Name}" }
				},
				RequestUnit = requestUnit,
				Expression = memberExpression,
				RequestContextSelector = configurations.ContainsKey(DocumentDbConfigurationExtensions.GetKey(DocumentDbConstants.RequestContextSelector, sourceType)) ?
					configurations.GetValue<Func<IGraphRequestContext, bool>>(DocumentDbConstants.RequestContextSelector, sourceType) : null
			};

			await _documentClient.CreateDocumentCollectionIfNotExistsAsync(
				UriFactory.CreateDatabaseUri(databaseId), new DocumentCollection
				{
					Id = documentTypeInfo.Id.ToLower(),
					PartitionKey = documentTypeInfo.Partition
				}, new RequestOptions
				{
					OfferThroughput = documentTypeInfo.RequestUnit
				});

			_documentTypeInfos.Add(documentTypeInfo);
		}

		public async Task CreateOrUpdateAsync<T>(T item, IGraphRequestContext graphRequestContext)
		{
			await _documentClient.UpsertDocumentAsync(
				GetDocumentCollectionUri<T>(graphRequestContext), GetTransformed(item), null, true);
		}

		public async Task CreateAsync<T>(T item, IGraphRequestContext graphRequestContext)
		{
			await _documentClient.CreateDocumentAsync(
				GetDocumentCollectionUri<T>(graphRequestContext), GetTransformed(item), null, true);
		}

		private dynamic GetTransformed<T>(T item)
		{
			dynamic expando = JsonConvert.DeserializeObject<ExpandoObject>(
				JsonConvert.SerializeObject(item));

			expando.id = item.GetKey();

			return expando;
		}

		private Uri GetDocumentCollectionUri<T>(IGraphRequestContext graphRequestContext)
		{
			var collectionName = GetCollectionName<T>();
			var databaseId = Get<T>(graphRequestContext).DatabaseId;

			return UriFactory.CreateDocumentCollectionUri(databaseId, collectionName);
		}

		private Uri GetDocumentUri<T>(T item, IGraphRequestContext graphRequestContext)
		{
			var collectionName = GetCollectionName<T>();
			var databaseId = Get<T>(graphRequestContext).DatabaseId;

			return UriFactory.CreateDocumentUri(databaseId, collectionName, item.GetKey());
		}

		private string GetCollectionName<T>()
		{
			return typeof(T).Name.ToLower();
		}

		public async Task UpdateAsync<T>(T item, IGraphRequestContext graphRequestContext)
		{
			await _documentClient.ReplaceDocumentAsync(
				GetDocumentUri(item, graphRequestContext), GetTransformed(item), await GetRequestOptions(item, graphRequestContext));
		}

		public bool CanHandle<T>(IGraphRequestContext graphRequestContext)
		{
			return InternalGet<T>(graphRequestContext) != null;
		}

		private DocumentTypeInfo Get<T>(IGraphRequestContext graphRequestContext)
		{
			var documentTypeInfo = InternalGet<T>(graphRequestContext);

			if (documentTypeInfo == null)
				throw new ApplicationException("Unable to determine the correct RequestContextSelector to process request.");

			return documentTypeInfo;
		}

		private DocumentTypeInfo InternalGet<T>(IGraphRequestContext graphRequestContext)
		{
			var documentTypeInfos = _documentTypeInfos.Where(x => x.Id == typeof(T).Name).ToList();
			if (documentTypeInfos.Count == 1 && documentTypeInfos.Single().RequestContextSelector == null)
				return documentTypeInfos.Single();

			documentTypeInfos = documentTypeInfos.Where(x =>
				x.RequestContextSelector != null && x.RequestContextSelector(graphRequestContext)).ToList();

			if (documentTypeInfos.Count == 1) return documentTypeInfos.Single();

			return null;
		}

		private async Task<RequestOptions> GetRequestOptions<T>(T item, IGraphRequestContext graphRequestContext)
		{
			var documentTypeInfo = Get<T>(graphRequestContext);
			if (documentTypeInfo != null)
			{
				var memberExpression = documentTypeInfo.Expression;
				var value = item.GetPropertyValue(memberExpression.Member.Name);

				if (value == null ||
					value is int intValue && intValue == 0 ||
					value is string strValue && strValue == "" ||
					value is Guid guidValue && guidValue == Guid.Empty)
				{
					// Let's query for the partition.
					var collection = new SqlParameterCollection { new SqlParameter("@id", item.GetKey()) };
					var sqlQuery = new SqlQuerySpec("SELECT * FROM x WHERE x.id=@id", collection);

					var query = _documentClient.CreateDocumentQuery<T>(GetDocumentCollectionUri<T>(graphRequestContext),
							sqlQuery,
							new FeedOptions { EnableCrossPartitionQuery = true })
						.AsDocumentQuery();

					var results = (await query.ExecuteNextAsync<T>()).ToList();

					if (results.Count > 1)
					{
						throw new InvalidOperationException(
							"Unable to determine partition key value due to multiple matches.");
					}

					if (results.Count == 1)
					{
						value = results.Single().GetPropertyValue(memberExpression.Member.Name);

						if (value == null)
						{
							throw new InvalidOperationException(
								$"Unable to determine partition key value with member expression: {memberExpression.Member.Name}");
						}
					}
					else
					{
						throw new InvalidOperationException(
							"Unable to determine partition key value due to missing data.");
					}
				}

				if (value is Guid)
				{
					value = value.ToString();
				}

				return new RequestOptions { PartitionKey = new PartitionKey(value) };
			}

			return null;
		}

		public async Task DeleteAsync<T>(T item, IGraphRequestContext graphRequestContext)
		{
			await _documentClient.DeleteDocumentAsync(
				GetDocumentUri(item, graphRequestContext), await GetRequestOptions(item, graphRequestContext));
		}

		private const string All = "*";

		public async Task<IEnumerable<T>> QueryAsync<T>(IEnumerable<QueryParameter> queryParameters, IGraphRequestContext graphRequestContext)
		{
			var collection = new SqlParameterCollection();

			var queryParametersList = queryParameters.ToList();

			string sql;

			if (queryParametersList.Count == 1 && (queryParametersList.Single().ContextValue == null || queryParametersList.Single().ContextValue.Values == null))
			{
				sql = $"SELECT {All} FROM x";
			}
			else
			{
				sql = $"SELECT {All} FROM x WHERE ";
				const string and = " AND ";

				queryParametersList.ForEach(x =>
				{
					var documentDbSqlParameter = TranslateQueryParameter(x);
					documentDbSqlParameter.SqlParameters.ToList().ForEach(collection.Add);
					sql += documentDbSqlParameter.Comparison + and;
				});

				if (sql.EndsWith(and))
					sql = sql.Substring(0, sql.LastIndexOf("AND ", StringComparison.Ordinal));
			}



			var sqlQuery = new SqlQuerySpec(sql, collection);

			var options = new FeedOptions
			{
				EnableCrossPartitionQuery = true,

				// See: https://docs.microsoft.com/en-us/azure/cosmos-db/index-policy

				EnableScanInQuery = queryParametersList.Any(
					x => x.ContextValue != null && (
						 x.ContextValue.Comparison == Comparisons.StringContains ||
						 x.ContextValue.Comparison == Comparisons.StringEndsWith ||
						 x.ContextValue.Comparison == Comparisons.StringStartsWith ||
						 x.ContextValue.Comparison == Comparisons.GreaterEqualThan ||
						 x.ContextValue.Comparison == Comparisons.GreaterThan ||
						 x.ContextValue.Comparison == Comparisons.LessThan ||
						 x.ContextValue.Comparison == Comparisons.LessEqualThan))
			};

			_logger.LogInformation($"Generated SQL query in DocumentDb provider: {sql}");

			var query = _documentClient.CreateDocumentQuery<T>(
				GetDocumentCollectionUri<T>(graphRequestContext), sqlQuery, options).AsDocumentQuery();

			var results = await query.ExecuteNextAsync<T>();
			return results.ToList();
		}

		private DocumentDbSqlParameter TranslateQueryParameter(QueryParameter queryParameter)
		{
			var comparison = _documentDbComparisons.FirstOrDefault(x => x.CanHandle(queryParameter));

			if (comparison == null) throw new NotImplementedException($"Comparison {queryParameter.ContextValue.Comparison} is not implemented for type.");

			var documentDbSqlParameter = comparison.Generate();

			if (documentDbSqlParameter == null) throw new NotImplementedException($"Comparison {queryParameter.ContextValue.Comparison} is not implemented by {comparison.GetType().FullName}.");

			return documentDbSqlParameter;
		}

		public async Task DeleteAllAsync<T>(IGraphRequestContext graphRequestContext)
		{
			var documentTypeInfo = Get<T>(graphRequestContext);

			var uri = UriFactory.CreateDocumentCollectionUri(documentTypeInfo.DatabaseId, documentTypeInfo.Id.ToLower());

			var options = new FeedOptions
			{
				MaxItemCount = 100
			};

			while (true)
			{
				var query = _documentClient.CreateDocumentQuery<T>(uri, options).AsDocumentQuery();

				var response = await query.ExecuteNextAsync<T>();

				if (response.Count == 0) break;

				foreach (var item in response)
				{
					await DeleteAsync(item, graphRequestContext);
				}

				if (string.IsNullOrEmpty(response.ResponseContinuation)) break;

				options.RequestContinuation = response.ResponseContinuation;
			}
		}
	}
}