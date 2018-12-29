[Main page](../README.md)

# Query introduction

Before creating your queries, be sure to have setup [mutation](Mutations.md) for your model type. 

The first step is to define a class for queries and associate it in your schema defination. Notice that you will need to inject the helper query builder class, QueryBuilderFactory. We will also need to provide a name. By convention, we can use the word query.
```
using GraphQL.Types;
using Microsoft.Extensions.Logging;

namespace Eklee.Examples
{
	public class MyQuery : ObjectGraphType<object>
	{
		public MyQuery(QueryBuilderFactory queryBuilderFactory)
		{
			Name = "query";
```

Now, you can apply query class defination in your schema defination.
```
using GraphQL;
using GraphQL.Types;

namespace Eklee.Examples
{
    public class MySchema : Schema
    {
        public BooksSchema(IDependencyResolver resolver, MyQuery myQuery, MyMutation myMutation) : base(resolver)
        {
            Query = myQuery;
```

## Simple Query by Id

This example shows how we can quickly setup a query by a Id field.
```
	queryBuilderFactory.Create<Book>(myQuery, "GetBookById")
		.WithParameterBuilder()
			.WithKeys()
			.BuildQuery()
		.BuildWithSingleResult();
```

There are a few things to note here. We start by providing a model type in which we would like to query for. In this context, it is a Book model.

Next, we pass in the MyQuery instance where the query is being applied. We also need to provide a name for the query.

After that, we are ready to start with defining the query parameters. This is where we start with the WithParameterBuilder extension method which gives us the ability to invoke WithKeys. As long as you have define the Key attribute on your model property, it will know how to use that property for performing a query based on Id.

Lastly, we are ready to build the query with BuildQuery extension method and close off by stating that we are only expect one result from the query using BuildWithSingleResult.

Let's take a look at a different example. This is equivalent to the following except you are the one defining the property in which to query on.
```
	queryBuilderFactory.Create<Book>(myQuery, "GetBookById")
		.WithParameterBuilder()
			.WithKeys()
			.BuildQuery()
		.BuildWithSingleResult();
```

** More documentation is coming. **