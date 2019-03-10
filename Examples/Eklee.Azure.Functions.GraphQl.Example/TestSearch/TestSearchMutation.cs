using Eklee.Azure.Functions.GraphQl.Example.Models;
using Eklee.Azure.Functions.GraphQl.Example.TestSearch.Models;
using GraphQL.Types;
using Microsoft.Extensions.Configuration;

namespace Eklee.Azure.Functions.GraphQl.Example.TestSearch
{
	public class TestSearchMutation : ObjectGraphType
	{
		public TestSearchMutation(InputBuilderFactory inputBuilderFactory, IConfiguration configuration)
		{
			Name = "mutation";

			var conn = configuration["TableStorage:ConnectionString"];

			inputBuilderFactory.Create<Model1>(this)
				.ConfigureTableStorage<Model1>()
				.AddConnectionString(conn)
				.AddPartition(x => x.Field)
				.BuildTableStorage()
				.DeleteAll(() => new Status { Message = "All Model1 have been removed." })
				.Build();

			inputBuilderFactory.Create<Model2>(this)
				.ConfigureTableStorage<Model2>()
				.AddConnectionString(conn)
				.AddPartition(x => x.Field)
				.BuildTableStorage()
				.DeleteAll(() => new Status { Message = "All Model2 have been removed." })
				.Build();

			inputBuilderFactory.Create<Model3>(this)
				.ConfigureTableStorage<Model3>()
				.AddConnectionString(conn)
				.AddPartition(x => x.Field)
				.BuildTableStorage()
				.DeleteAll(() => new Status { Message = "All Model3 have been removed." })
				.Build();

			var api = configuration["Search:ApiKey"];
			var serviceName = configuration["Search:ServiceName"];

			inputBuilderFactory.Create<MySearch>(this)
				.DeleteAll(() => new Status { Message = "All MySearch1 searches have been deleted." })
				.ConfigureSearchWith<MySearch, Model1>()
				.AddApiKey(api)
				.AddServiceName(serviceName)
				.AddPrefix("lcl1")
				.BuildSearch()
				.Build();

			inputBuilderFactory.Create<MySearch>(this)
				.DeleteAll(() => new Status { Message = "All MySearch1 searches have been deleted." })
				.ConfigureSearchWith<MySearch, Model2>()
				.AddApiKey(api)
				.AddServiceName(serviceName)
				.AddPrefix("lcl1")
				.BuildSearch()
				.Build();

			inputBuilderFactory.Create<MySearch>(this)
				.DeleteAll(() => new Status { Message = "All MySearch1 searches have been deleted." })
				.ConfigureSearchWith<MySearch, Model3>()
				.AddApiKey(api)
				.AddServiceName(serviceName)
				.AddPrefix("lcl1")
				.BuildSearch()
				.Build();
		}
	}
}
