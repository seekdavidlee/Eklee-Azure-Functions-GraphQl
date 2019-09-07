using Autofac;
using Eklee.Azure.Functions.GraphQl.Actions.RequestContextValueExtractors;
using Eklee.Azure.Functions.GraphQl.Example.Actions;
using Eklee.Azure.Functions.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace Eklee.Azure.Functions.GraphQl.Example.TestStorage.Core
{
	public class TestStorageFunctionModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.UseDistributedCache<MemoryDistributedCache>();
			builder.UseSystemModelTransformers();
			builder.UseValueFromRequestContextGenerator();
			builder.RegisterType<ValueFromRequestHeader>().As<IRequestContextValueExtractor>().SingleInstance();

			builder.RegisterGraphQl<TestStorageSchemaConfig>();
			builder.RegisterType<TestStorageQueryConfigObjectGraphType>();
			builder.RegisterType<TestStorageMutationObjectGraphType>();
		}
	}
}
