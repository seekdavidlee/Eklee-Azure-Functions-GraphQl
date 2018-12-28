using Autofac;
using Eklee.Azure.Functions.GraphQl.Example.HttpMocks;

namespace Eklee.Azure.Functions.GraphQl.Example
{
	public class MyModule2 : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<PublisherMockRepository>().As<IHttpMockRepository<Publisher>>().SingleInstance();
		}
	}
}