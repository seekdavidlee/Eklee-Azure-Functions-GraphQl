using Eklee.Azure.Functions.GraphQl.Connections;
using Eklee.Azure.Functions.GraphQl.Tests.Models;
using Shouldly;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests.Connections
{
	[Trait(Constants.Category, Constants.UnitTests)]
	public class ConnectionFieldResolverTests
	{
		private readonly ConnectionEdgeResolver _connectionFieldResolver =
			new ConnectionEdgeResolver();

		[Fact]
		public void NoExceptionsForANoneConnectionModel()
		{
			var edges = _connectionFieldResolver.HandleConnectionEdges(new NoneConnectionModel
			{
				Id = "1",
				Field1 = "f",
				Field2 = 1
			}, null);

			edges.Count.ShouldBe(0);
		}

		[Fact]
		public void CanResolveEdgeConnectionBetween2ModelsOfSameType()
		{
			const string srcId = "model1_1";
			const string destId = "model1_2";
			int counter = 0;
			var model1 = new Model1
			{
				Id = srcId,
				Field1 = "foo",
				Field2 = 3,

				BestFriend = new Model1Friend
				{
					Field1 = "edge foo",
					Field2 = 5,
					Id = destId,
					Model1 = new Model1
					{
						Id = destId,
						Field1 = "bar",
						Field2 = 4
					}
				}
			};

			Model1 m1_1 = null;
			Model1 m1_2 = null;

			var edges = _connectionFieldResolver.HandleConnectionEdges(model1, (entity) =>
			{
				if (entity is Model1 em)
				{
					if (em.Id == srcId)
						m1_1 = em;

					if (em.Id == destId)
						m1_2 = em;
				}

				counter++;
			});

			counter.ShouldBe(2);

			edges.ShouldNotBeNull();
			edges.Count.ShouldBe(1);

			m1_1.ShouldNotBeNull();
			m1_2.ShouldNotBeNull();

			edges[0].SourceId.ShouldBe(srcId);
			edges[0].DestinationId.ShouldBe(destId);

			edges[0].MetaFieldName.ShouldNotBeNull();
			edges[0].MetaFieldName.ShouldNotBeEmpty();

			edges[0].MetaType.ShouldNotBeNull();
			edges[0].MetaType.ShouldNotBeEmpty();

			edges[0].MetaValue.ShouldNotBeNull();
			edges[0].MetaValue.ShouldNotBeEmpty();
		}

		[Fact]
		public void CanResolveOneWayEdgeConnectionBetween2DifferentModelTypes()
		{
			const string srcId = "model2_1";
			const string destId = "model3_1";
			int counter = 0;

			Model2 m2_1 = null;
			Model3 m3_1 = null;

			var model2 = new Model2
			{
				Id = srcId,
				Field1 = "f1",
				Field2 = 34,
				Edge = new Model2ConnectionToModel3
				{
					Field1 = "f12",
					Id = destId,
					Model3 = new Model3
					{
						Id = destId,
						Field1 = "barr"
					}
				}
			};

			var edges = _connectionFieldResolver.HandleConnectionEdges(model2, (entity) =>
			{
				if (entity is Model2 em2)
				{
					if (em2.Id == srcId) m2_1 = em2;
				}

				if (entity is Model3 em3)
				{
					if (em3.Id == destId) m3_1 = em3;
				}

				counter++;
			});

			counter.ShouldBe(2);

			edges.ShouldNotBeNull();
			edges.Count.ShouldBe(1);

			m2_1.ShouldNotBeNull();
			m3_1.ShouldNotBeNull();

			edges[0].SourceId.ShouldBe(srcId);
			edges[0].DestinationId.ShouldBe(destId);

			edges[0].MetaFieldName.ShouldNotBeNull();
			edges[0].MetaFieldName.ShouldNotBeEmpty();

			edges[0].MetaType.ShouldNotBeNull();
			edges[0].MetaType.ShouldNotBeEmpty();

			edges[0].MetaValue.ShouldNotBeNull();
			edges[0].MetaValue.ShouldNotBeEmpty();
		}

		[Fact]
		public void ModelsOfSameTypeOnlyResolvedOnce()
		{
			const string srcId = "model1_1";
			const string destId = "model1_1";
			int counter = 0;
			var model1 = new Model1
			{
				Id = srcId,
				Field1 = "foo",
				Field2 = 3,

				BestFriend = new Model1Friend
				{
					Field1 = "I am my own friend!",
					Field2 = 5,
					Id = destId,
					Model1 = new Model1
					{
						Id = destId
					}
				}
			};

			Model1 m1_1 = null;
			Model1 m1_2 = null;

			var edges = _connectionFieldResolver.HandleConnectionEdges(model1, (entity) =>
			{
				if (entity is Model1 em)
				{
					if (em.Id == srcId)
						m1_1 = em;

					if (em.Id == destId)
						m1_2 = em;
				}

				counter++;
			});

			counter.ShouldBe(1);

			edges.ShouldNotBeNull();
			edges.Count.ShouldBe(1);

			m1_1.ShouldNotBeNull();
			m1_2.ShouldNotBeNull();

			edges[0].SourceId.ShouldBe(srcId);
			edges[0].DestinationId.ShouldBe(destId);

			edges[0].MetaFieldName.ShouldNotBeNull();
			edges[0].MetaFieldName.ShouldNotBeEmpty();

			edges[0].MetaType.ShouldNotBeNull();
			edges[0].MetaType.ShouldNotBeEmpty();

			edges[0].MetaValue.ShouldNotBeNull();
			edges[0].MetaValue.ShouldNotBeEmpty();
		}
	}
}
