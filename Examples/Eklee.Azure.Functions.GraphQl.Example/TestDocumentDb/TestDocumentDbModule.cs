using Autofac;
using Eklee.Azure.Functions.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace Eklee.Azure.Functions.GraphQl.Example.TestDocumentDb
{
	public class TestDocumentDbModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.UseDistributedCache<MemoryDistributedCache>();

			builder.RegisterGraphQl<TestDocumentDbSchema>();
			builder.RegisterType<TestDocumentDbQuery>();
			builder.RegisterType<TestDocumentDbMutation>();
			builder.UseDataAnnotationsValidation();
		}
	}
}
