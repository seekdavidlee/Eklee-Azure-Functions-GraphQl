# Introduction

The purpose of this library is to help developers with implementing their API on a GraphQL Server running on top of Azure HTTP Function. The library will have resolver support for different Azure-specific repositories like Azure Cosmos DB. If you are not sure what GraphQL is, the best resource would be to review the documentation on [https://graphql.org/](https://graphql.org/).

**Breaking Changes Notice, PLEASE READ**

**Starting from version 0.30, we have started using version 3.x of GraphQL.NET. GraphQL.NET has a new implementation for Connections which broke the ability to use a generic Typed class to wrap around a Model. Paging has been disabled because this incompatibility.**

**DateTime is now returning ISO 8601 format.**

**We are now on .NET Standard 2.1.**

## Nuget

You can find this library on nuget: [https://www.nuget.org/packages/Eklee.Azure.Functions.GraphQl](https://www.nuget.org/packages/Eklee.Azure.Functions.GraphQl).

## DevOps

- [https://dev.azure.com/eklee/Eklee.Azure.Functions.GraphQl](https://dev.azure.com/eklee/Eklee.Azure.Functions.GraphQl)

## Example Project(s)

- [Eklee-Azure-Functions-GraphQl-Examples](https://github.com/seekdavidlee/Eklee-Azure-Functions-GraphQl-Examples)
- [Eklee-Exams-Api](https://github.com/seekdavidlee/Eklee-Exams-Api)

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

### Misc

For more information about the library used for dependency injection support, check out: [https://github.com/seekdavidlee/Eklee-Azure-Functions-Http](https://github.com/seekdavidlee/Eklee-Azure-Functions-Http)

## Step 2: Setup HTTP function.

The second step is to apply the ExecutionContextDependencyInjection attribute on your HTTP triggered Function and tell it which Module to use. Next, you can inject the ExecutionContext which internally carries the function instance Id. Notice that by convention, we allow both HTTP GET and POST. This, by convention, is what is [recommended](https://graphql.org/learn/serving-over-http/) by GraphQL. 

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

The power of GraphQL is that we are able to serve the API via a single HTTP endpoint and consumers need to only know to query for the schema on this endpoint and perform query or mutation operations. Thus, we are giving a generic Route name here called Graph. However, you may want to give it a more domain specific name if you intend to have more than one endpoint.


## Step 3: Implement GraphQL server:

Simply leverage the extension method ProcessGraphQlRequest with the HTTP request which will be processed by the GraphQL server.

```
return await executionContext.ProcessGraphQlRequest(req);
```

### Next Steps: Setup Models and update Mutation and Query classes.

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

# Other topics:

- [Tracing](Documentation/Tracing.md)
- [Query Caching](Documentation/Caching.md)
- [Model Validation](Documentation/Validations.md)
- [Model Transforms](Documentation/Transforms.md)
- [Connecting Models](Documentation/Connections.md)
- [Search with Aggregations](Documentation/Searches.md)
- [Mutation lifecycle hooks](Documentation/MutationActions.md)

# Recommanded tools:

- [GraphQL Playground](https://github.com/prisma/graphql-playground/releases): GraphQL IDE that can consume local or remote schema, execute queries and mutations etc.
- [GraphQL CLI tool](https://github.com/graphql-cli/graphql-cli): Generates a local GraphQL schema file for use with tools such as GraphQL Playground.
- [Cosmos DB Emulator](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator)
- [Storage Emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator)