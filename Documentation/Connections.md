[Main page](../README.md)

# Introduction

When 2 Models are related to each other via some form of relationship, we can easily create this relationship via the use of the Connection attribute. 

## Connection Attribute

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

# Other Notes

* The Connection concept is currently ONLY supported with the use of CosmosDb. It is not available in other types of Data Sources. We will be adding the other Data Sources shortly.
* It can potentially get complex if you decide to find all the best friends a few more levels deep. This has the effect of slowing down the response time as a query is executed for each level.