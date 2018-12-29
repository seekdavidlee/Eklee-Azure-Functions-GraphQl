# Introduction

The purpose of this library is to help developers with implementing their API on a GraphQL Server running on top of Azure HTTP Function. The library will have resolver support for different Azure-specific repositories like Azure Cosmos DB. If you are not sure what GraphQL is, the best resource would be to review the documentation on [https://graphql.org/](https://graphql.org/).

## Nuget

You can find this library on nuget: [https://www.nuget.org/packages/Eklee.Azure.Functions.GraphQl](https://www.nuget.org/packages/Eklee.Azure.Functions.GraphQl).

# Getting started

Let's start by exposing a HTTP Function to serve your API via GraphQL server. There are 3 steps.

## Step 1: Setup dependency injection (DI)

The first step is to setup your DI via the Autofac Module. Be sure to register your schema using the extension method RegisterGraphQl. You can then register your mutation and query used in the schema. Please refer to the topics below for specific details on setting up your mutations and query classes.

- [Mutations](Documentation/Mutations.md)
- [Queries](Documentation/Queries.md)

```
using Autofac;

namespace Eklee.Examples
{
    public class MyModule : Module
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

## Caching:

To setup caching, in your Module setup, use the extension method UseDistributedCache. Note that MemoryDistributedCache is just an example. In a production senario, you may choose something like Azure Redis.

```
builder.UseDistributedCache<MemoryDistributedCache>();
```

## Misc

For more information about the library used for dependency injection support, check out: [https://github.com/seekdavidlee/Eklee-Azure-Functions-Http](https://github.com/seekdavidlee/Eklee-Azure-Functions-Http)

## Step 2: Setup HTTP function.

The second step is to apply the ExecutionContextDependencyInjection attribute on your HTTP triggered Function and tell it which Module to use. Next, you can inject the ExecutionContext which internally carries the function instance Id. Notice that by convention, we allow both HTTP GET and POST. This is by convention what is [recommended](https://graphql.org/learn/serving-over-http/) by GraphQL. The power of GraphQL is that we are able to serve the API via a single HTTP endpoint and consumers need to only know to query for the schema on this endpoint and perform query or mutation operations. Thus, we are giving a generic Route name here called Graph. However, you may want to give it a more domain specific name if you intend to have more than one endpoint.

```
public static class MyGraphFunction
{
    [ExecutionContextDependencyInjection(typeof(MyModule))]
    [FunctionName("graph")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "graph")] HttpRequest req,
        ILogger log,
        ExecutionContext executionContext)
    {	
```

## Step 3: Implement ProcessGraphQlRequest:

Simply leverage the extension method ProcessGraphQlRequest. Internally, this is the GraphQL server.

```
return await executionContext.ProcessGraphQlRequest(req);
```

## Next Steps: Setup Models and update Mutation and Query classes.

We use a Model-First (with Fluent syntax) to define the GraphQL schema. Description is a required attribute on the model which provides documentation for the model property.

```
using System.ComponentModel.DataAnnotations;
...
    public class Book
    {
        [Key]
        [Description("Id of the book")]
        public string Id { get; set; }

        [Description("Name of the book")]
        public string Name { get; set; }
```

Once we have completed these steps, we are ready to start running the Azure HTTP Function.

# Tracing support:

To enable support for tracing, please add set EnableMetrics configuration to true under GraphQl.

```
{
    ...
    "GraphQl": {
      "EnableMetrics": "true" 
    } 
}
```

** More documentation/topics are coming. **