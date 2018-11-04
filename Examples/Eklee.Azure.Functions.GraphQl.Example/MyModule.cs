using Autofac;
using Eklee.Azure.Functions.GraphQl.Example.BusinessLayer;
using Microsoft.Extensions.Caching.Distributed;

namespace Eklee.Azure.Functions.GraphQl.Example
{
    public class MyModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.EnableGraphQlCache<MemoryDistributedCache>();
            builder.RegisterGraphQl<BooksSchema>();
            builder.RegisterType<BooksQuery>();
            builder.RegisterType<BooksRepository>().SingleInstance();
            builder.RegisterType<BookType>();
            builder.RegisterType<BooksMutation>();
            builder.RegisterType<BookInputType>();
            builder.EnableGraphQlPagingFor<BookType>();
        }
    }
}