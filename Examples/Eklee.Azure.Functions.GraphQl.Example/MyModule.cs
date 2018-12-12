using Autofac;
using Eklee.Azure.Functions.GraphQl.Example.BusinessLayer;
using Eklee.Azure.Functions.GraphQl.Example.HttpMocks;
using Eklee.Azure.Functions.GraphQl.Repository;
using Eklee.Azure.Functions.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace Eklee.Azure.Functions.GraphQl.Example
{
	public class MyModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.UseDistributedCache<MemoryDistributedCache>();

			builder.RegisterGraphQl<BooksSchema>();
			builder.RegisterType<BooksQuery>();
			builder.RegisterType<BooksMutation>();

			builder.RegisterType<InMemoryRepository>().As<IGraphQlRepository>().SingleInstance();
			builder.RegisterType<HttpRepository>().As<IGraphQlRepository>().SingleInstance();
			builder.RegisterType<GraphQlRepositoryProvider>().As<IGraphQlRepositoryProvider>().SingleInstance();

			builder.RegisterType<PublisherMockRepository>().As<IHttpMockRepository<Publisher>>().SingleInstance();
		}
	}
}