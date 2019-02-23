using Autofac;
using Eklee.Azure.Functions.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace Eklee.Azure.Functions.GraphQl.Example.TestSearch
{
	public class TestSearchModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.UseDistributedCache<MemoryDistributedCache>();

			builder.RegisterGraphQl<TestSearchSchema>();
			builder.RegisterType<TestSearchQuery>();
			builder.RegisterType<TestSearchMutation>();
		}
	}
}
