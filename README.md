# Introduction

The purpose of this library is to help developers with implementing a GraphQl based Azure Function with dependency injection support.

## DI Usage

In order to leverage this library, there are 3 steps. You would want to setup your DI, apply the ExecutionContextDependencyInjection attribute, and inject the ExecutionContext as a parameter in your function.

### Step 1: Setup DI

The first step is to setup your DI via the Autofac Module. Be sure to register your schema using the extension method RegisterGraphQl. You can then register the types used in your schema.

```
using Autofac;

namespace FunctionApp1
{
    public class MyModuleConfig : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGraphQl<BooksSchema>();
            builder.RegisterType<BooksQuery>();
            ...
        }
    }
}
```

### Step 2/3: Setup ExecutionContextDependencyInjection attribute on said function and inject ExecutionContext.

The second step is to apply the ExecutionContextDependencyInjection on your function and tell it which Module type you would like. Next, you can inject the ExecutionContext which internally carries the function instance Id.

```
public static class BooksGraphFunction
{
    [ExecutionContextDependencyInjection(typeof(MyModule))]
    [FunctionName("graph")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "books/graph")] HttpRequest req,
        ILogger log,
        ExecutionContext executionContext)
    {	
```

## Process GraphQl Request Usage:

Simply leverage the extension method ProcessGraphQlRequest. 

```
return await executionContext.ProcessGraphQlRequest(req);
```

For more information about dependency injection support, visit: https://github.com/seekdavidlee/Eklee-Azure-Functions-Http

## Caching + Paging Usage:

You can enable built-in caching capabilities per GraphQl best practices based on object type and instance Id. Please follow the steps below:

In your Module setup, use the extension method EnableGraphQlCache. Note that MemoryDistributedCache is just an example. In a production senario, you may choose something like Azure Redis.

```
builder.EnableGraphQlCache<MemoryDistributedCache>();
```

There's no need to setup for Paging.

You can use a convention based approach to identify cache instances based on object type and instance Id by decorating with the KeyAttribute on your object type.

```
using System.ComponentModel.DataAnnotations;
...
    public class Book
    {
        [Key]
        public string Id { get; set; }
```

In your Query resolvers, you can use the extension method ResolverWithCache to create a resolver with caching support. Once the cache expires, the respository query will be executed and persisted into the cache for the duration of time specified.

Please refer to specific examples below for implementation usage.

```
using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl.Example.BusinessLayer
{
    public class BooksQuery : ObjectGraphType<object>
    {
        public BooksQuery(BooksRepository booksRepository, IGraphQlCache graphQlCache)
        {
            Name = "Query";

            // Example 1: We are getting a single Book. You are defining the argument yourself to pass into the repository with context. There's no caching and paging support. This is what comes out-of-the-box.

            Field<BookType>("book_nocache",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "id of the book" }),
                resolve: context => booksRepository.GetBook(context.GetArgument<string>("id")));

            // Example 2: We are getting a single Book. The argument to pass into the repository is defined by the Model with at least one property with the KeyAttribute.
            //            The work is done by the cache repository which will cache the book result for a specific time you have defined. There's no paging support.

            Field<BookType>("book",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "id of the book" }),
                resolve: graphQlCache.ResolverWithCache(key => booksRepository.GetBook((string)key), 10));


            // Example 3: We are getting a list of Books based on an argument. You are defining the key to pass into the repository without having to use context directly.
            //            The cache repository which will cache the book result for a specific time you have defined. There's no paging support.

            Field<ListGraphType<BookType>>("books_category",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "category", Description = "category of the book" }),
                resolve: graphQlCache.ResolverWithCache(key => booksRepository.GetBooks((string)key), 10, "category"));

            // Example 4: We are getting a list of paged Books. Technically, you are able to get all books by using TotalCount, although there's already a default page limit of 10 items per page if you don't specify.
            //            There's no caching support.

            Connection<BookType>().Name("booksConnection_nocache")
                .ResolveAsync(async context => await context.GetConnectionAsync(booksRepository.GetBooks()));

            // Example 5: We are getting a list of paged Books with a argument to be passed in. You are defining the argument yourself to pass into the repository with context.
            //            Technically, you are able to get all books by using TotalCount, although there's already a default page limit of 10 if you don't specify. There's no caching support.

            Connection<BookType>().Name("books_categoryConnection_nocache")
                .Argument<NonNullGraphType<StringGraphType>>("category", "category of the book")
                .ResolveAsync(async context => await context.GetConnectionAsync(booksRepository.GetBooks(context.GetArgument<string>("category"))));

            // Example 6: We are getting a list of paged Books with a argument to be passed in. You are defining the key to pass into the repository without having to use context directly.
            //            The cache repository which will cache the book result for a specific time you have defined. You will get paged results with a default page limit of 10 items per page if you don't specify.

            Connection<BookType>().Name("books_categoryConnection")
                .Argument<NonNullGraphType<StringGraphType>>("category", "category of the book")
                .ResolveAsync(async context => await context.GetConnectionWithCacheAsync(graphQlCache, key => booksRepository.GetBooks((string)key), "category"));

        }
    }
}
```

## Tracing support:

To enable support for tracing, please add set EnableMetrics configuration to true under GraphQl.

```
{
    ...
    "GraphQl": {
      "EnableMetrics": "true" 
    } 
}
```