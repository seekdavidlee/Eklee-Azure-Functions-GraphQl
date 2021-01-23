using Azure;
using Azure.Search.Documents.Indexes;
using Eklee.Azure.Functions.GraphQl.Repository.Search;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Example.Operations
{
	public class Operations : IOperations
	{
		private readonly IConfiguration _configuration;

		public Operations(IConfiguration configuration)
		{
			_configuration = configuration;
		}
		public async Task DeleteSearchIndexes()
		{
			var key = _configuration["Search:ApiKey"];
			var serviceName = _configuration["Search:ServiceName"];

			var searchCredentials = new AzureKeyCredential(key);
			var searchServiceClient = new SearchIndexClient(serviceName.GetSearchServiceUri(), searchCredentials);

			// Unable to use GetIndexNamesAsync due to the following: https://github.com/Azure/azure-sdk-for-net/issues/15590
			var result = searchServiceClient.GetIndexesAsync();

			await foreach (var index in result)
			{
				await searchServiceClient.DeleteIndexAsync(index.Name);
			}
		}
	}
}
