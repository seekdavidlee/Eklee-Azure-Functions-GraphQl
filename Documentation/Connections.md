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

# Other Note

The Connection concept is currently ONLY supported with the use of CosmosDb. It is not available in other types of Data Sources. We will be adding the other Data Sources shortly.