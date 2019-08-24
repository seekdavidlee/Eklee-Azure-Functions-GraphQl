using Autofac;
using Eklee.Azure.Functions.GraphQl;
using Eklee.Azure.Functions.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace Eklee.Azure.Functions.GraphQl.Example.TestStorage.Core
{
	public class TestStorageFunctionModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.UseDistributedCache<MemoryDistributedCache>();

			builder.RegisterGraphQl<TestStorageSchemaConfig>();
			builder.RegisterType<TestStorageQueryConfigObjectGraphType>();
			builder.RegisterType<TestStorageMutationObjectGraphType>();
		}
	}
}
