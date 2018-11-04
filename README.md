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

## Caching Usage:

You can enable built-in caching capabilities per GraphQl best practices based on object type and instance Id. Please follow the steps below:

In your Module setup, use the extension method EnableGraphQlCache. Note that MemoryDistributedCache is just an example. In a production senario, you may choose something like Azure Redis.

```
builder.EnableGraphQlCache<MemoryDistributedCache>();
```

You can use a convention based approach to identify cache instances based on object type and instance Id by decorating with the KeyAttribute on your object type.

```
using System.ComponentModel.DataAnnotations;
...
    public class Book
    {
        [Key]
        public string Id { get; set; }
```

In your Query resolvers, you can use the extension method ResolverWithCache to create a resolver with caching support. 

The first "book" query is based on the Book object type's Id property to derive "id" from the context as a key to get the instance Id value. It is cached for 10 seconds.

The second "books" query will derive "category" from the context as a key to get the instance Id value. It is cached for 10 seconds.

Once the cache expires, the respository query will be executed and persisted into the cache for the duration of time specified.

```
using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl.Example.BusinessLayer
{
    public class BooksQuery : ObjectGraphType<object>
    {
        public BooksQuery(BooksRepository booksRepository, IGraphQlCache graphQlCache)
        {
            Name = "Query";

            Field<BookType>("book",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "id of the book" }),
                resolve: graphQlCache.ResolverWithCache(key => booksRepository.GetBook((string)key), 10));

            Field<ListGraphType<BookType>>("books",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "category", Description = "category of the book" }),
                resolve: graphQlCache.ResolverWithCache(key => booksRepository.GetBooks((string)key), 10, "category"));

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