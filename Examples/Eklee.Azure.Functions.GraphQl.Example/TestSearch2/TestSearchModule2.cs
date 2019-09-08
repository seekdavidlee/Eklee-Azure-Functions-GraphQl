using Autofac;
using Eklee.Azure.Functions.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace Eklee.Azure.Functions.GraphQl.Example.TestSearch2
{
	public class TestSearchModule2 : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.UseDistributedCache<MemoryDistributedCache>();
			builder.UseSystemModelTransformers();
			builder.RegisterGraphQl<TestSearchSchema2>();
			builder.RegisterType<TestSearchQuery2>();
			builder.RegisterType<TestSearchMutation2>();
		}
	}
}
