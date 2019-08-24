using GraphQL.Types;
using Microsoft.Extensions.Configuration;
using Eklee.Azure.Functions.GraphQl.Connections;
using Eklee.Azure.Functions.GraphQl.Example.Models;

namespace Eklee.Azure.Functions.GraphQl.Example.TestStorage.Core
{
	public class TestStorageMutationObjectGraphType : ObjectGraphType
	{
		public TestStorageMutationObjectGraphType(InputBuilderFactory inputBuilderFactory, IConfiguration configuration)
		{
			Name = "mutation";

			inputBuilderFactory.Create<ConnectionEdge>(this)
				.ConfigureTableStorage<ConnectionEdge>()
				.AddConnectionString(configuration["TableStorage:ConnectionString"])
				.AddPartition(x => x.SourceId)
				.BuildTableStorage()
				.Build();

			inputBuilderFactory.Create<Model7>(this)
				.ConfigureTableStorage<Model7>()
				.AddConnectionString(configuration["TableStorage:ConnectionString"])
				.AddPartition(x => x.Field)
				.BuildTableStorage()
				.DeleteAll(() => new Status { Message = "All Model7 have been removed." })
				.DisableBatchCreate()   // Note it would be hard to use batch create in this context. Use batchCreateOrUpdate instead.
				.Build();

			inputBuilderFactory.Create<Model8>(this)
				.ConfigureTableStorage<Model8>()
				.AddConnectionString(configuration["TableStorage:ConnectionString"])
				.AddPartition(x => x.Field)
				.BuildTableStorage()
				.DeleteAll(() => new Status { Message = "All Model8 have been removed." })
				.DisableBatchCreate()   // Note it would be hard to use batch create in this context. Use batchCreateOrUpdate instead.
				.Build();
		}
	}
}
