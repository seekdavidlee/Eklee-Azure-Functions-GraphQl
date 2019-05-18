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

			AssertEdge(edges[0], srcId, destId);
		}

		private void AssertEdge(ConnectionEdge edge, string srcId, string destId)
		{
			edge.Id.ShouldNotBeNull();
			edge.Id.ShouldNotBeEmpty();

			edge.SourceFieldName.ShouldNotBeNullOrEmpty();
			edge.SourceType.ShouldNotBeNullOrEmpty();

			edge.SourceId.ShouldBe(srcId);
			edge.DestinationId.ShouldBe(destId);

			edge.MetaFieldName.ShouldNotBeNullOrEmpty();
			edge.MetaType.ShouldNotBeNullOrEmpty();
			edge.MetaValue.ShouldNotBeNullOrEmpty();
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


		[Fact]
		public void CanResolve3EdgeConnection()
		{
			const string srcId = "conn_1";
			const string destId1 = "conn_2_1";
			const string destId2 = "conn_2_2";
			const string destId3 = "conn_2_3";
			int counter = 0;

			var model = new ModelWith3Connections
			{
				Id = srcId,
				Field1 = "fas",
				Edge1 = new ModelWith3ConnectionsEdge
				{
					Id = destId1,
					Field1 = "das one",
					Other = new ModelWith3ConnectionsOther { Id = destId1 }
				},
				Edge2 = new ModelWith3ConnectionsEdge
				{
					Id = destId2,
					Field1 = "das two",
					Other = new ModelWith3ConnectionsOther { Id = destId2 }
				},
				Edge3 = new ModelWith3ConnectionsEdge
				{
					Id = destId3,
					Field1 = "das three",
					Other = new ModelWith3ConnectionsOther { Id = destId3 }
				}
			};

			ModelWith3Connections m1 = null;
			ModelWith3ConnectionsOther m2_1 = null;
			ModelWith3ConnectionsOther m2_2 = null;
			ModelWith3ConnectionsOther m2_3 = null;

			var edges = _connectionFieldResolver.HandleConnectionEdges(model, (entity) =>
			{
				if (entity is ModelWith3Connections em1)
				{
					if (em1.Id == srcId) m1 = em1;
				}

				if (entity is ModelWith3ConnectionsOther em2)
				{
					if (em2.Id == destId1) m2_1 = em2;
					if (em2.Id == destId2) m2_2 = em2;
					if (em2.Id == destId3) m2_3 = em2;
				}

				counter++;
			});

			counter.ShouldBe(4);

			edges.ShouldNotBeNull();
			edges.Count.ShouldBe(3);

			m1.ShouldNotBeNull();
			m2_1.ShouldNotBeNull();
			m2_2.ShouldNotBeNull();
			m2_3.ShouldNotBeNull();

			int errorCount = 0;

			edges.ForEach(edge =>
			{
				if (edge.DestinationId == destId1)
				{
					AssertEdge(edge, srcId, destId1);
					return;
				}

				if (edge.DestinationId == destId2)
				{
					AssertEdge(edge, srcId, destId2);
					return;
				}

				if (edge.DestinationId == destId3)
				{
					AssertEdge(edge, srcId, destId3);
					return;
				}

				errorCount++;

			});

			errorCount.ShouldBe(0);
		}

		[Fact]
		public void CanResolveWithAtLeastOneNullEdgeConnection()
		{
			const string srcId = "conn_1";
			const string destId1 = "conn_2_1";
			const string destId2 = "conn_2_2";

			int counter = 0;

			var model = new ModelWith3Connections
			{
				Id = srcId,
				Field1 = "fas",
				Edge1 = new ModelWith3ConnectionsEdge
				{
					Id = destId1,
					Field1 = "das one",
					Other = new ModelWith3ConnectionsOther { Id = destId1 }
				},
				Edge2 = new ModelWith3ConnectionsEdge
				{
					Id = destId2,
					Field1 = "das two",
					Other = new ModelWith3ConnectionsOther { Id = destId2 }
				},
				Edge3 = null
			};

			ModelWith3Connections m1 = null;
			ModelWith3ConnectionsOther m2_1 = null;
			ModelWith3ConnectionsOther m2_2 = null;

			var edges = _connectionFieldResolver.HandleConnectionEdges(model, (entity) =>
			{
				if (entity is ModelWith3Connections em1)
				{
					if (em1.Id == srcId) m1 = em1;
				}

				if (entity is ModelWith3ConnectionsOther em2)
				{
					if (em2.Id == destId1) m2_1 = em2;
					if (em2.Id == destId2) m2_2 = em2;
				}

				counter++;
			});

			counter.ShouldBe(3);

			edges.ShouldNotBeNull();
			edges.Count.ShouldBe(2);

			m1.ShouldNotBeNull();
			m2_1.ShouldNotBeNull();
			m2_2.ShouldNotBeNull();

			int errorCount = 0;

			edges.ForEach(edge =>
			{
				if (edge.DestinationId == destId1)
				{
					AssertEdge(edge, srcId, destId1);
					return;
				}

				if (edge.DestinationId == destId2)
				{
					AssertEdge(edge, srcId, destId2);
					return;
				}

				errorCount++;

			});

			errorCount.ShouldBe(0);
		}
	}
}
