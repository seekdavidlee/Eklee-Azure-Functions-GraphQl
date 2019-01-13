[Main page](../README.md)

# Mutation introduction

Mutation is the process by which data is ingested/updated/deleted in the system.

The first step is to define a class for mutations and associate it in our schema defination. Notice that we will need to inject the helper input builder class, InputBuilderFactory. We will also need to provide a name. By convention, we can use the word mutations.
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

Now, we can apply mutation class defination in our schema defination.
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

Lastly, please note to register MyMutation in the AutoFac Module as part of our dependency injection setup.

# Repositories

With mutation also comes the concept of where the entity type data is persisted to. This is why we have created different repositories.

Currently, the following repository types are supported.

- HttpRepository
- DocumentDbRepository (Azure CosmosDb, SQL API)
- InMemoryRepository

## In-memory based mutation

Let's start with a in-memory example. In this example, we are creating a mutation for which our Create<T> extension method will generate the schema for Batch Create, Single Create, Update and Delete for our model.
```
inputBuilderFactory.Create<Book>(this)
	.Delete<BookId, Status>(
		bookInput => new Book { Id = bookInput.Id },
		book => new Status { Message = $"Successfully removed book with Id {book.Id}" })
	.ConfigureInMemory<Book>().BuildInMemory()
	.DeleteAll(() => new Status { Message = "All books have been removed." })
	.Build();
```

There are a few things to note here. We start by providing a model type in which we would like to create the muation for. In this context, it is a Book model. There's an interesting take on the Delete. We are actually passing back a different model called Status. Actually, it can be anything. For the sake of our example, we are just passing back a status with a message indicating the Book model has been deleted. We are free to suggest any type of return type.

Next, we are ready to configure the repository for the model type. It is important to note that we allow only a single repository source per model type. In this case, it is a InMemory repository which does not require any additional configuraton. As we add more repository, there is going to be repository specific configurations. Because we are using the Fluent syntax, it is easy to discover what those configuration options are.

Notice the DeleteAll method. Again, we have an interesting take on Delete. For local testing, it may be useful to enable this feature so that we have the ability to perform cleanup activities. We may want to add specific code such that this feature is turned off in Production senarios.

Lastly, call the Build extension method to set this up. 

The in-memory example is useful if we are starting to learn about GraphQL and wish to have a quick way to learn about this library quickly, or, if we wish to have a simple way to test out our models and APIs.

## HTTP based mutation

The HTTP mutation allows us to define the HTTP URL and HTTP Verbs to apply when we are performing a CRUD operation using a ConfigureHttp<T> extension method. Let's review the example below.
```
const string publishersResource = "publishers";

inputBuilderFactory.Create<Publisher>(this)
	.ConfigureHttp<Publisher>()
		.AddBaseUrl("http://localhost:7071/api/")
		.AddResource(publisher => new HttpResource { AppendUrl = publishersResource, Method = HttpMethod.Post })
		.UpdateResource(publisher => new HttpResource { AppendUrl = $"{publishersResource}/{publisher.Id}", Method = HttpMethod.Put })
		.DeleteResource(publisher => new HttpResource { AppendUrl = $"{publishersResource}/{publisher.Id}", Method = HttpMethod.Delete })
		.QueryResource(PublisherQueryExtensions.GetPublisherByIdQuery, items => new HttpQueryResource
		{
			AppendUrl = $"{publishersResource}/{items["id"]}",
			QueryType = HttpQueryTypes.AppendToUrl
		})
		.DeleteAllResource(() => new HttpResource { AppendUrl = publishersResource, Method = HttpMethod.Delete })
		.BuildHttp()
	.DeleteAll(() => new Status { Message = "All publishers have been removed." })
	.Build();
```

The first thing we are doing is to help define a single base URL using the AddBaseUrl extension method. It is assumed that the Type is going to target a single endpoint. 

Next, we start calling each individual CRUD based extension methods to define the HTTP URL and Verb based on an instance of T that is being passed back from the GraphQL engine.

When we are done, we call the BuildHttp to close the HTTP build configuration. Again, the DeleteAll is optional, for testing purposes typically.

Lastly, call the Build extension method to set this up.

## Azure Cosmos DB (SQL API/ Document database) based mutation

The Azure Cosmos DB consist of multiple types of data services like Document database, Graph database etc. This section relates specifically to the Document database. To begin, we start by calling the ConfigureDocumentDb<T> extension method.  Let's review the example below where we are setting up the Delete configuration first.
```
inputBuilderFactory.Create<BookReview>(this)
	.Delete<BookReviewId, Status>(
		bookReviewInput => new BookReview { Id = bookReviewInput.Id },
		bookReview => new Status { Message = $"Successfully removed book review with Id {bookReview.Id}" })
	.ConfigureDocumentDb<BookReview>()
		.AddUrl(documentDbUrl)
		.AddKey(documentDbKey)
		.AddDatabase(rc => "local")
		.AddRequestUnit(400)
		.AddPartition(bookReview => bookReview.ReviewerId)
		.BuildDocumentDb()
	.DeleteAll(() => new Status { Message = "All book reviews relationships have been removed." })
	.Build();
```

Azure Cosmos DB requires a URL, key, and a database name to begin with. Internally, if the database does not exist, we will automatically create the database using the URL and key provided. This is why the Request Unit configuration setting might apply.

Next, we need to provide a partition key based on the Type. 

Finally, we can call the BuildDocumentDb extension method to close the document db build configuration.

## Azure Search based mutation

In order to leverage Azure Search, we will need to create an instance of Azure Search and provide the Service name and key. The following is an example. Note that the Model can be created (inhertied) from an existing type. In the example below, ConfigureSearch is used to configure Azure Search with the Service name and key.

```
inputBuilderFactory.Create<BookSearch>(this)
	.DeleteAll(() => new Status { Message = "All book searches have been deleted." })
	.ConfigureSearch<BookSearch>()
	.AddApiKey(configuration["Search:ApiKey"])
	.AddServiceName(configuration["Search:ServiceName"])
	.BuildSearch()
	.Build();
```

### Sync Azure Search with Model

It is possible to perform a mutation with a Model and the corresponding Search Model will also be either created, updated or deleted. See the following example.

```
inputBuilderFactory.Create<ReviewerSearch>(this)
	.DeleteAll(() => new Status { Message = "All reviewer searches have been deleted." })
	.ConfigureSearchWith<ReviewerSearch, Reviewer>()
	.AddApiKey(configuration["Search:ApiKey"])
	.AddServiceName(configuration["Search:ServiceName"])
	.BuildSearch()
	.Build();
```

The ConfigureSearchWith<TSearchModel,TModel> will allow you to associate a Search Model with an existing Model type.
