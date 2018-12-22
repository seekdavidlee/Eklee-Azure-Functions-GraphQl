using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GraphQL;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;

namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public class DocumentTypeInfo
	{
		public PartitionKeyDefinition Partition { get; set; }
		public int RequestUnit { get; set; }
		public string Id { get; set; }
	}

	public class DocumentClientProvider
	{
		private readonly IEnumerable<IDocumentDbComparison> _documentDbComparisons;
		private const int DefaultRequestUnits = 400;
		private readonly DocumentClient _documentClient;

		private readonly Dictionary<string, string> _databases = new Dictionary<string, string>();

		private readonly Dictionary<string, MemberExpression> _memberExpressions =
			new Dictionary<string, MemberExpression>();

		private readonly Dictionary<string, DocumentTypeInfo> _documentTypeInfos =
			new Dictionary<string, DocumentTypeInfo>();

		public DocumentClientProvider(string url, string key, IEnumerable<IDocumentDbComparison> documentDbComparisons)
		{
			_documentDbComparisons = documentDbComparisons;
			_documentClient = new DocumentClient(new Uri(url), key);
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

			if (!_databases.ContainsKey(databaseId))
			{
				await _documentClient.CreateDatabaseIfNotExistsAsync(
						new Database { Id = databaseId },
						new RequestOptions { OfferThroughput = requestUnit });
			}

			_databases[sourceType.Name.ToLower()] = databaseId;


			var memberExpression = configurations.GetValue<MemberExpression>(
				DocumentDbConstants.PartitionMemberExpression, sourceType);

			var documentTypeInfo = new DocumentTypeInfo
			{
				Id = sourceType.Name.ToLower(),
				Partition = new PartitionKeyDefinition
				{
					Paths = new Collection<string> { $"/{memberExpression.Member.Name}" }
				},
				RequestUnit = requestUnit
			};

			await _documentClient.CreateDocumentCollectionIfNotExistsAsync(
				UriFactory.CreateDatabaseUri(databaseId), new DocumentCollection
				{
					Id = documentTypeInfo.Id,
					PartitionKey = documentTypeInfo.Partition
				}, new RequestOptions
				{
					OfferThroughput = documentTypeInfo.RequestUnit
				});

			_documentTypeInfos.Add(sourceType.Name, documentTypeInfo);

			_memberExpressions.Add(sourceType.Name, memberExpression);
		}

		public async Task CreateAsync<T>(T item)
		{
			await _documentClient.CreateDocumentAsync(
				GetDocumentCollectionUri<T>(), GetTransformed(item), null, true);
		}

		private dynamic GetTransformed<T>(T item)
		{
			dynamic expando = JsonConvert.DeserializeObject<ExpandoObject>(
				JsonConvert.SerializeObject(item));

			expando.id = item.GetKey();

			return expando;
		}

		private Uri GetDocumentCollectionUri<T>()
		{
			var collectionName = GetCollectionName<T>();
			var databaseId = _databases[collectionName];

			return UriFactory.CreateDocumentCollectionUri(databaseId, collectionName);
		}

		private Uri GetDocumentUri<T>(T item)
		{
			var collectionName = GetCollectionName<T>();
			var databaseId = _databases[collectionName];

			return UriFactory.CreateDocumentUri(databaseId, collectionName, item.GetKey());
		}

		private string GetCollectionName<T>()
		{
			return typeof(T).Name.ToLower();
		}

		public async Task UpdateAsync<T>(T item)
		{
			await _documentClient.ReplaceDocumentAsync(
				GetDocumentUri(item), GetTransformed(item), await GetRequestOptions(item));
		}

		private async Task<RequestOptions> GetRequestOptions<T>(T item)
		{
			if (_memberExpressions.ContainsKey(typeof(T).Name))
			{
				var memberExpression = _memberExpressions[typeof(T).Name];
				var value = item.GetPropertyValue(memberExpression.Member.Name);

				if (value == null ||
					value is int intValue && intValue == 0 ||
					value is string strValue && strValue == "")
				{
					// Let's query for the partition.

					var query = _documentClient.CreateDocumentQuery<T>(GetDocumentCollectionUri<T>(),
						$"SELECT * FROM x WHERE x.id='{item.GetKey()}'",
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
					}
					else
					{
						throw new InvalidOperationException(
							"Unable to determine partition key value due to missing data.");
					}
				}

				return new RequestOptions { PartitionKey = new PartitionKey(value) };
			}

			return null;
		}

		public async Task DeleteAsync<T>(T item)
		{
			await _documentClient.DeleteDocumentAsync(
				GetDocumentUri(item), await GetRequestOptions(item));
		}

		public async Task<IEnumerable<T>> QueryAsync<T>(IEnumerable<QueryParameter> queryParameters)
		{
			var sql = "SELECT * FROM x WHERE ";
			const string and = " AND ";

			var queryParametersList = queryParameters.ToList();
			queryParametersList.ForEach(x => sql += TranslateQueryParameter(x) + and);

			if (sql.EndsWith(and))
				sql = sql.Substring(0, sql.LastIndexOf("AND ", StringComparison.Ordinal));

			var options = new FeedOptions
			{
				EnableCrossPartitionQuery = true,

				// See: https://docs.microsoft.com/en-us/azure/cosmos-db/index-policy

				EnableScanInQuery = queryParametersList.Any(
					x => x.Comparison == Comparisons.StringContains ||
						 x.Comparison == Comparisons.StringEndsWith ||
						 x.Comparison == Comparisons.StringStartsWith)
			};

			var query = _documentClient.CreateDocumentQuery<T>(
				GetDocumentCollectionUri<T>(), sql, options).AsDocumentQuery();

			var results = await query.ExecuteNextAsync<T>();
			return results.ToList();
		}

		private string TranslateQueryParameter(QueryParameter queryParameter)
		{
			var comparison = _documentDbComparisons.FirstOrDefault(x => x.CanHandle(queryParameter));

			if (comparison == null) throw new NotImplementedException($"Comparison {queryParameter.Comparison} is not implemented for type.");

			var statement = comparison.Generate();

			if (statement == null) throw new NotImplementedException($"Comparison {queryParameter.Comparison} is not implemented by {comparison.GetType().FullName}.");

			return statement;
		}

		public async Task DeleteAllAsync<T>()
		{
			var documentTypeInfo = _documentTypeInfos[typeof(T).Name];

			var uri = UriFactory.CreateDocumentCollectionUri(
				_databases[typeof(T).Name.ToLower()], documentTypeInfo.Id);

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
					await DeleteAsync(item);
				}

				if (string.IsNullOrEmpty(response.ResponseContinuation)) break;

				options.RequestContinuation = response.ResponseContinuation;
			}
		}
	}
}