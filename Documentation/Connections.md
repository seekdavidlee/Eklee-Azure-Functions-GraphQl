[Main page](../README.md)

# Introduction

When 2 Models are related to each other via some form of relationship, we can easily create this relationship via the use of the Connection attribute. 

## Connection attribute

Let's take the following as an example.

```
public class Model5
{
	[Key]
	[Description("Id")]
	public string Id { get; set; }

	[Description("IntField")]
	public int IntField { get; set; }

	[Description("DoubleField")]
	public double DoubleField { get; set; }

	[Description("DateField")]
	public DateTime DateField { get; set; }

	[Description("Field")]
	public string Field { get; set; }

	[Connection]
	[Description("A Best Friend")]
	public Model5Friend BestFriend { get; set; }

	[Connection]
	[Description("A Close Friend")]
	public Model5Friend CloseFriend { get; set; }
}
```

Model5 indicates that there are 2 relationships - one being a "BestFriend" and one being a "GoodFriend". Both have Type of Model5Friend which is an "edge" or relationship defination. The following demostrates this relationship class.

```
public class Model5Friend
{
	[ConnectionEdgeDestinationKey]
	[Description("Id of Model 1, The Friend.")]
	public string Id { get; set; }

	[Description("Field")]
	public string Field { get; set; }

	[ConnectionEdgeDestination]
	[Description("TheFriend")]
	public Model5 TheFriend { get; set; }
}
```

As we can observe, there's an ConnectionEdgeDestinationKey attribute as well as a ConnectionEdgeDestination attribute. The ConnectionEdgeDestinationKey is used to define the Id of the other end of the relationship which is defined by ConnectionEdgeDestination. In this case, it is another Model5! Note that it is not necessary to populate the Model5 property. As long as we have set the Id, it is sufficent to establish the relationship.

The relationship is established with the use of a model which is called ConnectionEdge. This means we will need to define this model in our [mutation class defination](Mutations.md).

```
inputBuilderFactory.Create<ConnectionEdge>(this)
	.ConfigureDocumentDb<ConnectionEdge>()
	.AddKey(key)
	.AddUrl(url)
	.AddRequestUnit(requestUnits)
	.AddDatabase(db)
	.AddPartition(x => x.SourceId)
	.BuildDocumentDb()
	.Build();
```

Also note that other fields on Model5Friend such as Field can be used to describe the relationship. Imagine being able to say that the "friendship" started on a certain date. This means we can add a new Started date field on this class.

## Query

The power of building the relationship comes from being able to now query them out. 

```
query{
  searchModel5(id:{ equal: "model5_1" }){
    id
    field
    intField
    closeFriend {
      id
      theFriend {
        id
      }
    }
    bestFriend {
      id
      field 
      theFriend {
        id
        bestFriend {
          id
        }
      }
    }
  }
}
```

Now, we can search for Model5 and find Model5's best friend. Let's take a look at an example output.

```
{
  "data": {
    "searchModel5": {
      "id": "model5_1",
      "field": "do",
      "intField": 6,
      "closeFriend": {
        "id": "model_4",
        "theFriend": {
          "id": "model_4"
        }
      },
      "bestFriend": {
        "id": "model5_2",
        "field": "ray",
        "theFriend": {
          "id": "model5_2",
          "bestFriend": {
            "id": "model5_3"
          }
        }
      }
    }
  },
  "extensions": {}
}
```

When we found model5_1, we can see model5_1's best friend to be model5_2. We can continue the chain and find model5_2's best friend who is actually model5_3. Notice that we can keep going down and chain if we want to.

## Query ConnectionEdge to find Source entity

In cases where you know the destination Id, but would like to figure out what Source entities are associated with this, you can use the WithDestinationId to build up a query.

Let's take the following structure where Model7 and Model8 are related via Connection type Model7ToModel8.

```
public class Model7
{
	[Key]
	[Description("Id")]
	public string Id { get; set; }

	[Description("Field")]
	public string Field { get; set; }

	[Connection]
	[Description("Model7ToModel8")]
	public Model7ToModel8 Model7ToModel8 { get; set; }
}
```

```
public class Model8
{
	[Key]
	[Description("Id")]
	public string Id { get; set; }

	[Description("Field")]
	public string Field { get; set; }
}
```

```
public class Model7ToModel8
{
	[ConnectionEdgeDestinationKey]
	[Description("Id of destination")]
	public string Id { get; set; }

	[Description("Field")]
	public string Field { get; set; }

	[ConnectionEdgeDestination]
	[Description("TheModel8")]
	public Model8 TheModel8 { get; set; }
}
```

Let's run the following mutations.

```
mutation {
  batchCreateOrUpdateModel7(model7:[{
    id:"model7_1"
    field:"model7_1"
    model7ToModel8 :{
      id:"model8_1"
      field:"model8_1_conn_to_model7_1"
      theModel8:{
        id:"model8_1"
        field:"model8_1"
      }
    }
  },
{
    id:"model7_2"
    field:"model7_2"
    model7ToModel8 :{
      id:"model8_2"
      field:"model8_2_conn_to_model7_2"
      theModel8:{
        id:"model8_2"
        field:"model8_2"
      }
    }
  },
{
    id:"model7_3"
    field:"model7_3"
    model7ToModel8 :{
      id:"model8_2"
      field:"model8_2_conn_to_model7_3"
      theModel8:{
        id:"model8_2"
        field:"model8_2"
      }
    }
  }    
  
  ]){
    id
  }
}
```

Now, we can build the following query.

```
queryBuilderFactory.Create<Model7>(this, "GetModel7WithModel8Id", "Get Model7")
	.WithParameterBuilder()
	.WithConnectionEdgeBuilder<Model7ToModel8>()
		.WithDestinationId()
	.BuildConnectionEdgeParameters()
	.BuildQuery()
	.BuildWithListResult();
```

In the example above, we can see that the Model7 is the source entity. If we know the Id for Model8, we wouldn't be able to easily get to Model8. However, with WithConnectionEdgeBuilder, we can just define the query with DestinationId and now we can query for Model7.

```
query{
  getModel7WithModel8Id(destinationid:{ equal:"model8_2" }){
  	id
    field
  }
}
```

The result would look like the following:

```
{
  "data": {
    "getModel7WithModel8Id": [
      {
        "id": "model7_2",
        "field": "model7_2"
      },
      {
        "id": "model7_3",
        "field": "model7_3"
      }
    ]
  },
  "extensions": {}
}
```
## Query ConnectionEdge Model

We can also query on the ConnectionEdge Model itself. This uses an in-memory provider to do the additional filtering.

With the following code, notice the line ```.WithProperty(x => x.FieldDescription)```. This is used to additionally filter on the connection Model Model7ToModel8. 
```
// Find Model7 coming from Model8
queryBuilderFactory.Create<Model7>(this, "GetModel7WithModel8FieldAndConnectionFieldDescription", "Get Model7 With Model8 Field And Connection Field Description")
	.WithParameterBuilder()
	.BeginQuery<Model8>()
	// Use field from Model8 as a starting point to search from.
	.WithProperty(x => x.Field)
	.BuildQueryResult(ctx =>
	{
		ctx.Items["model8IdList"] = ctx.GetQueryResults<Model8>().Select(x => (object)x.Id).ToList();
	})
	.WithConnectionEdgeBuilder<Model7ToModel8>()
		// Now, we match Model7ToModel8's DestinationId with Model8 Id.
		.WithDestinationIdFromSource(ctx =>
		{
			return (List<object>)ctx.Items["model8IdList"];
		})
		.WithProperty(x => x.FieldDescription)
		.BuildConnectionEdgeParameters(ctx =>
		{
			ctx.Items["model7IdList"] = ctx.GetQueryResults<Model7ToModel8>().Select(x => x.Id)
				.Distinct()
				.Select(x => (object)x).ToList();
		})
	.ThenWithQuery<Model7>()
	.WithPropertyFromSource(x => x.Id, ctx =>
	{
		return (List<object>)ctx.Items["model7IdList"];
	})
	.BuildQueryResult(ctx => { })
	.BuildQuery()
	.BuildWithListResult();
```

The query itself will also now expose the field fielddescription for query filtering.
```
query {
  getModel7WithModel8FieldAndConnectionFieldDescription(field: {equal:"model8_2"},
  fielddescription : {equal:"model8_2_conn_to_model7_3"}){
    id
    field
    model7ToModel8 {
      id
      fieldDescription
      theModel8 {
        id
        field
      }
    }
  }
}
```

## Query ConnectionEdge Model with SourceId

There may be instances where the user is performing a search (which doesn't use connection model). In this senario, you may want to query the connection edge yourself to figure out the child node hanging on the connection model. This is where ```WithSourceIdFromSource<T>``` may help you where T is the Source Model you are looking for using Source Id obtained from the search results.

# Other notes

* The Connection concept is currently ONLY supported with the use of CosmosDb. It is partially supported in Azure Table Storage and not available in other types of Data Sources. We will be adding the other Data Sources shortly.
* It can potentially get complex if you decide to find all the best friends a few more levels deep. This has the effect of slowing down the response time as a query is executed for each level.