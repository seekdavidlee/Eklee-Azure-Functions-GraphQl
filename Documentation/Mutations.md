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

# Repositories

With mutation also comes the concept of where the entity type data is persisted to. This is why we have created different repositories.

Currently, the following repository types are supported.

- HttpRepository
- DocumentDbRepository (Azure CosmosDb, SQL API)
- InMemoryRepository

## Simple in-memory mutation

Let's start with a simple example. In this example, we are creating a mutation for which our Create extension method will generate the schema for Batch Create, Single Create, Update and Delete for our model.

```
	inputBuilderFactory.Create<Book>(this)
		.Delete<BookId, Status>(
			bookInput => new Book { Id = bookInput.Id },
			book => new Status { Message = $"Successfully removed book with Id {book.Id}" })
		.ConfigureInMemory<Book>().BuildInMemory()
		.DeleteAll(() => new Status { Message = "All books have been removed." })
		.Build();
```

There are a few things to note here. We start by providing a model type in which we would like to create the muation for. In this context, it is a Book model. There's an interesting take on the Delete. We are actually passing back a different model called Status. Actually, it can be anything. For the sake of our example, we are just passing back a status with a message indicating the Book model has been deleted. You are free to suggest any type of return type.

Next, we are ready to configure the repository for the model type. It is important to note that we allow only a single repository source per model type. In this case, it is a InMemory repository which does not require any additional configuraton. As we add more repository, there is going to be repository specific configurations. Because we are using the Fluent syntax, it is easy to discover what those configuration options are.

Notice the DeleteAll method. Again, we have an interesting take on Delete. For local testing, it may be useful to enable this feature so that you have the ability to perform cleanup activities. You may want to add specific code such that this feature is turned off in Production senarios.

Lastly, call the Build extension method to set this up.

** More documentation is coming. **