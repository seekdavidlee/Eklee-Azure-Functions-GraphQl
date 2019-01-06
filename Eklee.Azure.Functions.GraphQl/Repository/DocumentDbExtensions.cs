using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Eklee.Azure.Functions.GraphQl
{
	public static class DocumentDbExtensions
	{
		private const int MaximumCreateDatabaseRetryCount = 3;

		public static async Task CreateDatabaseIfNotExistsWithRetryAsync(
			this DocumentClient documentClient, string databaseId, int requestUnit)
		{
			int tryCounter = 0;
			while (true)
			{
				try
				{
					await documentClient.CreateDatabaseIfNotExistsAsync(
						new Database { Id = databaseId },
						new RequestOptions { OfferThroughput = requestUnit });

					break;
				}
				catch (DocumentClientException e)
				{
					if (tryCounter < MaximumCreateDatabaseRetryCount && e.Message.Contains("Unknown server error"))
					{
						tryCounter++;
						Thread.Sleep(tryCounter * 500);
					}
					else
					{
						throw;
					}
				}
			}
		}
	}
}
