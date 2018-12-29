[Main page](../README.md)

# Mutation introduction

Mutation is the process by which data is ingested/updated/deleted in the system.

The first step is to define a class for mutations and associate it in your schema defination. Notice that you will need to inject the helper input builder class, InputBuilderFactory. We will also need to provide a name. By convention, we can use the word mutations.

```
using GraphQL.Types;
using Microsoft.Extensions.Configuration;

namespace Eklee.Azure.Functions.GraphQl.Example.BusinessLayer
{
	public class MyMutation : ObjectGraphType
	{
		public BooksMutation(InputBuilderFactory inputBuilderFactory, IConfiguration configuration)
		{
			Name = "mutations";
```

Now, you can apply mutation class defination in your schema defination.
```
using GraphQL;
using GraphQL.Types;

namespace Eklee.Examples
{
    public class MySchema : Schema
    {
        public BooksSchema(IDependencyResolver resolver, MyQuery myQuery, MyMutation myMutation) : base(resolver)
        {
            Mutation = myMutation;
```

Lastly, please note to register MyMutation in your AutoFac Module as part of your dependency injection setup.

## Repositories

With mutation also comes the concept of where the entity type data is persisted to. This is why we have created different repositories.

Currently, the following repository types are supported.

- HttpRepository
- DocumentDbRepository (Azure CosmosDb, SQL API)
- InMemoryRepository

** More documentation is coming. **