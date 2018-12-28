[Back](../README.md)

# Introduction

Before creating your queries, be sure to have setup mutation for your model type. 

The first step to creating a query is to define a class for queries and associate it in your schema defination. Notice that you will need to inject the helper query builder class, QueryBuilderFactory.
```
using GraphQL.Types;
using Microsoft.Extensions.Logging;

namespace Eklee.Examples
{
	public class MyQuery : ObjectGraphType<object>
	{
		public MyQuery(QueryBuilderFactory queryBuilderFactory)
		{
		...
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

This example shows how we can quickly setup a query by a Id field. Notice that it uses the extension method WithKeys? As long as you have define the Key attribute on your model property, it will know how to use that property for performing a query based on Id.
```
	queryBuilderFactory.Create<Book>(booksQuery, "GetBookById")
		.WithParameterBuilder()
			.WithKeys()
			.BuildQuery()
		.BuildWithSingleResult();
```