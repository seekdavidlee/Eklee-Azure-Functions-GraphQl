using Autofac;
using Eklee.Azure.Functions.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace Eklee.Azure.Functions.GraphQl.Example.TestInMemory
{
	public class TestInMemoryModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.UseDistributedCache<MemoryDistributedCache>();

			builder.RegisterGraphQl<TestInMemorySchema>();
			builder.RegisterType<TestInMemoryQuery>();
			builder.RegisterType<TestInMemoryMutation>();
		}
	}
}
