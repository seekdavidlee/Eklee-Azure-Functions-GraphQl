using Autofac;
using Eklee.Azure.Functions.GraphQl.Example.TestDocumentDb.Events;
using Eklee.Azure.Functions.GraphQl.Example.Validations;
using Eklee.Azure.Functions.GraphQl.Repository;
using Eklee.Azure.Functions.GraphQl.Validations;
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
			builder.RegisterType<MyValidation>().As<IModelValidation>();

			builder.RegisterType<BarMutationPreAction>().As<IMutationPreAction>();
			builder.RegisterType<BarMutationPostAction>().As<IMutationPostAction>();

			builder.RegisterType<FooMutationPreAction>().As<IMutationPreAction>();
			builder.RegisterType<FooMutationPostAction>().As<IMutationPostAction>();
		}
	}
}
