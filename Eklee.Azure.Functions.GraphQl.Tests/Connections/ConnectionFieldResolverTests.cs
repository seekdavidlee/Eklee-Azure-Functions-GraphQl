using Eklee.Azure.Functions.GraphQl.Connections;
using Eklee.Azure.Functions.GraphQl.Tests.Models;
using Shouldly;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests.Connections
{
	[Trait(Constants.Category, Constants.UnitTests)]
	public class ConnectionFieldResolverTests
	{
		private readonly ConnectionFieldResolver _connectionFieldResolver =
			new ConnectionFieldResolver();

		[Fact]
		public void CanResolveEdgeConnectionBetween2ModelsRepresentedAsParentChild()
		{
			var model1 = new Model1
			{
				Id = "model1",
				Field1 = "foo",
				Field2 = 3,

				FriendEdge = new Model2
				{
					Id = "model2",
					Field1 = "bar",
					Field2 = 4,
					Edge = new Model1
					{
						Id = "model1"
					}
				},
				FriendEdgeMeta = new Model1Model2AreFriends
				{
					Field1 = "Good friends",
					Field2 = 5
				}
			};

			Model1 m1 = null;
			Model2 m2 = null;

			var list = _connectionFieldResolver.HandleConnectionEdges(model1, (entity) =>
			{
				if (entity is Model1 em1)
				{
					m1 = em1;
				}

				if (entity is Model2 em2)
				{
					m2 = em2;
				}
			});

			list.ShouldNotBeNull();
			list.Count.ShouldBe(1);

			m1.ShouldNotBeNull();
			m2.ShouldNotBeNull();

			list[0].SourceId.ShouldBe(model1.Id);
			list[0].DestinationId.ShouldBe(model1.FriendEdge.Id);
		}
	}
}
