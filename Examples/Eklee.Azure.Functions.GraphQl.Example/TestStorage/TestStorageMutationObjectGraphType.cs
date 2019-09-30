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

			inputBuilderFactory.Create<Model9>(this)
				.ConfigureTableStorage<Model9>()
				.AddConnectionString(configuration["TableStorage:ConnectionString"])
				.AddPartition(x => x.Field)
				.BuildTableStorage()
				.DeleteAll(() => new Status { Message = "All Model9 have been removed." })
				.DisableBatchCreate()   // Note it would be hard to use batch create in this context. Use batchCreateOrUpdate instead.
				.Build();

			inputBuilderFactory.Create<Model10>(this)
				.ConfigureTableStorage<Model10>()
				.AddConnectionString(configuration["TableStorage:ConnectionString"])
				.AddPartition(x => x.Field)
				.BuildTableStorage()
				.DeleteAll(() => new Status { Message = "All Model10 have been removed." })
				.DisableBatchCreate()   // Note it would be hard to use batch create in this context. Use batchCreateOrUpdate instead.
				.Build();

			inputBuilderFactory.Create<Model11>(this)
				.ConfigureTableStorage<Model11>()
				.AddConnectionString(configuration["TableStorage:ConnectionString"])
				.AddPartition(x => x.Field)
				.BuildTableStorage()
				.DeleteAll(() => new Status { Message = "All Model11 have been removed." })
				.DisableBatchCreate()   // Note it would be hard to use batch create in this context. Use batchCreateOrUpdate instead.
				.Build();

			inputBuilderFactory.Create<Model12>(this)
				.ConfigureTableStorage<Model12>()
				.AddConnectionString(configuration["TableStorage:ConnectionString"])
				.AddPartition(x => x.Field)
				.BuildTableStorage()
				.DeleteAll(() => new Status { Message = "All Model12 have been removed." })
				.DisableBatchCreate()   // Note it would be hard to use batch create in this context. Use batchCreateOrUpdate instead.
				.Build();

			inputBuilderFactory.Create<Model13Parent>(this)
				.ConfigureTableStorage<Model13Parent>()
				.AddConnectionString(configuration["TableStorage:ConnectionString"])
				.AddPartition(x => x.AccountId)
				.BuildTableStorage()
				.DeleteAll(() => new Status { Message = "All Model13Parent have been removed." })
				.DisableBatchCreate()   // Note it would be hard to use batch create in this context. Use batchCreateOrUpdate instead.
				.Build();

			inputBuilderFactory.Create<Model13Child>(this)
				.ConfigureTableStorage<Model13Child>()
				.AddConnectionString(configuration["TableStorage:ConnectionString"])
				.AddPartition(x => x.AccountId)
				.BuildTableStorage()
				.DeleteAll(() => new Status { Message = "All Model13Child have been removed." })
				.DisableBatchCreate()   // Note it would be hard to use batch create in this context. Use batchCreateOrUpdate instead.
				.Build();

			inputBuilderFactory.Create<Model14>(this)
				.ConfigureTableStorage<Model14>()
				.AddPrefix("Stg")
				.AddConnectionString(configuration["TableStorage:ConnectionString"])
				.AddPartition(x => x.Descr)
				.BuildTableStorage()
				.DeleteAll(() => new Status { Message = "All Model14 have been removed." })
				.DisableBatchCreate()   // Note it would be hard to use batch create in this context. Use batchCreateOrUpdate instead.
				.Build();
		}
	}
}
