using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GraphQL;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public class DocumentClientProvider
	{
		private const int DefaultRequestUnits = 400;
		private readonly DocumentClient _documentClient;
		private readonly Dictionary<string, string> _databases = new Dictionary<string, string>();
		private readonly Dictionary<string, MemberExpression> _memberExpressions =
			new Dictionary<string, MemberExpression>();

		public DocumentClientProvider(string url, string key)
		{
			_documentClient = new DocumentClient(new Uri(url), key);
		}

		public void ConfigureDatabaseAndCollection(Dictionary<string, object> configurations, Type sourceType)
		{
			int requestUnit = DefaultRequestUnits;

			var databaseId = configurations.GetStringValue(DocumentDbConstants.Database, sourceType);

			if (configurations.ContainsKey(DocumentDbConfigurationExtensions.GetKey(DocumentDbConstants.RequestUnit, sourceType)))
			{
				int.TryParse(configurations.GetStringValue(DocumentDbConstants.RequestUnit, sourceType), out requestUnit);
			}

			if (!_databases.ContainsKey(databaseId))
			{
				_documentClient.CreateDatabaseIfNotExistsAsync(
						new Database { Id = databaseId },
						new RequestOptions { OfferThroughput = requestUnit })
					.GetAwaiter().GetResult();
			}

			_databases[sourceType.Name.ToLower()] = databaseId;

			var partitionKey =
				configurations.GetValue<PartitionKeyDefinition>(DocumentDbConstants.Partition, sourceType);

			_documentClient.CreateDocumentCollectionIfNotExistsAsync(
				UriFactory.CreateDatabaseUri(databaseId), new DocumentCollection
				{
					Id = sourceType.Name.ToLower(),
					PartitionKey = partitionKey
				}, new RequestOptions
				{
					OfferThroughput = requestUnit

				})
				.GetAwaiter().GetResult();

			var memberExpressionKey = DocumentDbConfigurationExtensions.GetKey(DocumentDbConstants.MemberExpression, sourceType);

			if (configurations.ContainsKey(memberExpressionKey))
				_memberExpressions.Add(sourceType.Name, (MemberExpression)configurations[memberExpressionKey]);
		}

		public async Task CreateAsync<T>(T item)
		{
			await _documentClient.CreateDocumentAsync(GetDocumentCollectionUri<T>(), GetTransformed(item), null, true);
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
			await _documentClient.ReplaceDocumentAsync(GetDocumentUri(item), GetTransformed(item));
		}

		public async Task DeleteAsync<T>(T item)
		{
			PartitionKey partitionKey = null;
			if (_memberExpressions.ContainsKey(typeof(T).Name))
			{
				var memberExpression = _memberExpressions[typeof(T).Name];
				var value = item.GetPropertyValue(memberExpression.Member.Name);
				partitionKey = new PartitionKey(value);
			}

			await _documentClient.DeleteDocumentAsync(GetDocumentUri(item), new RequestOptions
			{
				PartitionKey = partitionKey
			});
		}
	}

	public class DocumentDbRepository : IGraphQlRepository
	{
		private readonly Dictionary<string, DocumentClientProvider> _providers = new Dictionary<string, DocumentClientProvider>();
		private readonly Dictionary<string, DocumentClientProvider> _typedProviders = new Dictionary<string, DocumentClientProvider>();

		public void Configure(Type sourceType, Dictionary<string, object> configurations)
		{
			var url = configurations.GetStringValue(DocumentDbConstants.Url, sourceType);

			var provider = _providers.ContainsKey(url)
				? _providers[url]
				: new DocumentClientProvider(url, configurations.GetStringValue(DocumentDbConstants.Key, sourceType));

			provider.ConfigureDatabaseAndCollection(configurations, sourceType);

			_providers.Add(url, provider);

			_typedProviders[sourceType.Name] = provider;
		}

		public async Task BatchAddAsync<T>(IEnumerable<T> items)
		{
			var provider = _typedProviders[typeof(T).Name];

			foreach (var item in items)
			{
				await provider.CreateAsync(item);
			}
		}

		public async Task AddAsync<T>(T item)
		{
			var provider = _typedProviders[typeof(T).Name];

			await provider.CreateAsync(item);
		}

		public async Task UpdateAsync<T>(T item)
		{
			var provider = _typedProviders[typeof(T).Name];

			await provider.UpdateAsync(item);
		}

		public async Task DeleteAsync<T>(T item)
		{
			var provider = _typedProviders[typeof(T).Name];

			await provider.DeleteAsync(item);
		}

		public Task<IEnumerable<T>> QueryAsync<T>(string queryName, IEnumerable<QueryParameter> queryParameters)
		{
			throw new NotImplementedException();
		}
	}
}
