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