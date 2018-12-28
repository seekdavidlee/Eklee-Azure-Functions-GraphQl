# Introduction

The purpose of this library is to help developers with implementing a GraphQl based Azure Function with dependency injection support. If you are not sure what GraphQL is, the best resource would be to review the documentation on [https://graphql.org/](https://graphql.org/).

## Nuget

You can find this library on nuget: [https://www.nuget.org/packages/Eklee.Azure.Functions.GraphQl](https://www.nuget.org/packages/Eklee.Azure.Functions.GraphQl).

## Getting started

In order to leverage this library, there are 3 steps. You would want to setup your DI, apply the ExecutionContextDependencyInjection attribute, and inject the ExecutionContext as a parameter in your function.

### Step 1: Setup dependency injection (DI)

The first step is to setup your DI via the Autofac Module. Be sure to register your schema using the extension method RegisterGraphQl. You can then register your mutation and query used in the schema.

```
using Autofac;

namespace FunctionApp1
{
    public class MyModuleConfig : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGraphQl<MySchema>();
            builder.RegisterType<MyQuery>();
            builder.RegisterType<MyMutation>();
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

For more information about dependency injection support, visit: [https://github.com/seekdavidlee/Eklee-Azure-Functions-Http](https://github.com/seekdavidlee/Eklee-Azure-Functions-Http)

## Caching:

In your Module setup, use the extension method EnableGraphQlCache. Note that MemoryDistributedCache is just an example. In a production senario, you may choose something like Azure Redis.

```
builder.UseDistributedCache<MemoryDistributedCache>();
```

Results are cached based on query parameters and return type. The query parameters are matched exactly. Thus if the same query is being executed, it will be returned from cache. You can tell if a cache key being used simply by looking at the Azure function console log output when running locally.

## Data Annotations:

We used a Model-first with Fluent syntax to define GraphQL schema. The description is a required attribute on the model.

```
using System.ComponentModel.DataAnnotations;
...
    public class Book
    {
        [Key]
        [Description("Id of the book")]
        public string Id { get; set; }
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

## Topics
- [Mutations](Documentation/Mutations.md)
- [Queries](Documentation/Queries.md)

** More documentation/topics are coming. **