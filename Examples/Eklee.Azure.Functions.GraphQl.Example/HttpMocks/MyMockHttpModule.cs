using Autofac;

namespace Eklee.Azure.Functions.GraphQl.Example.HttpMocks
{
	public class MyMockHttpModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<PublisherMockRepository>().As<IHttpMockRepository<Publisher>>().SingleInstance();
		}
	}
}
