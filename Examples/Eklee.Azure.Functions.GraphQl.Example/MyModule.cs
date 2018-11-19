using Autofac;
using Eklee.Azure.Functions.GraphQl.Example.BusinessLayer;
using Eklee.Azure.Functions.Http;
using GraphQL.Types.Relay;
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
	        builder.RegisterType<QueryBuilderFactory>();
	        builder.RegisterType<InputBuilderFactory>();
	        builder.RegisterType<BooksMutation>();

	        builder.RegisterType<BookIdInputType>();

	        builder.RegisterGeneric(typeof(ModelConventionInputType<>));
	        builder.RegisterGeneric(typeof(ModelConventionType<>));
	        builder.RegisterGeneric(typeof(ConnectionType<>));
	        builder.RegisterType<PageInfoType>();
	        builder.RegisterGeneric(typeof(EdgeType<>));

	        builder.RegisterType<StatusType>();

	        builder.RegisterType<InMemoryDbRepository>().As<IGraphQlRepository>().SingleInstance();
	        builder.RegisterType<MyStartup>().As<IStartable>().SingleInstance();
		}
    }
}