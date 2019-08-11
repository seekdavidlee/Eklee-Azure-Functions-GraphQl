using Microsoft.Azure.Search;
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

			var searchCredentials = new SearchCredentials(key);
			var searchServiceClient = new SearchServiceClient(serviceName, searchCredentials);

			var result = await searchServiceClient.Indexes.ListNamesAsync();

			foreach (var name in result)
			{
				await searchServiceClient.Indexes.DeleteAsync(name);
			}
		}
	}
}
