using Eklee.Azure.Functions.GraphQl.Connections;
using Eklee.Azure.Functions.GraphQl.Repository;
using Eklee.Azure.Functions.GraphQl.Tests.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests.Connections
{
	[Trait(Constants.Category, Constants.UnitTests)]
	public class ConnectionEdgeHandlerTests
	{
		private readonly IGraphQlRepositoryProvider _graphQlRepositoryProvider;
		private readonly IConnectionEdgeHandler _connectionEdgeHandler;
		private IGraphQlRepository _connectionEdgeRepository;

		public ConnectionEdgeHandlerTests()
		{
			_graphQlRepositoryProvider = Substitute.For<IGraphQlRepositoryProvider>();
			_connectionEdgeRepository = Substitute.For<IGraphQlRepository>();

			_graphQlRepositoryProvider.GetRepository<ConnectionEdge>().Returns(_connectionEdgeRepository);

			_connectionEdgeHandler = new ConnectionEdgeHandler(_graphQlRepositoryProvider, new ConnectionEdgeResolver(), Substitute.For<ILogger>());
		}

		private List<object> PrepData()
		{
			var model5_a = new ModelWith3Connections();
			model5_a.Field1 = "a";
			model5_a.Id = "model5_a";

			var model5_b = new ModelWith3Connections();
			model5_b.Field1 = "b";
			model5_b.Id = "model5_b";

			return new List<object>
			{
				model5_a,
				model5_b
			};
		}

		private QueryStep PrepQuery(SelectValue selectValue)
		{
			var qs = new QueryStep();
			qs.QueryParameters.Add(new QueryParameter
			{
				ContextValue = new ContextValue
				{
					SelectValues = new List<SelectValue>
					{
						new SelectValue { FieldName = "Id" },
						new SelectValue {FieldName = "Field1" }
					},
					Comparison = Comparisons.Equal
				}
			});

			qs.QueryParameters.First().ContextValue.SelectValues.Add(selectValue);

			return qs;
		}

		[Fact]
		public async Task OneLevelQueryWorks()
		{
			var list = PrepData();
			var qs = PrepQuery(new SelectValue());

			await _connectionEdgeHandler.QueryAsync(list, qs, null, null, 
				new List<ConnectionEdgeDestinationFilter>());

			list.Count.ShouldBe(2);

			var m5_a = GetModel("model5_a", list);
			m5_a.ShouldNotBeNull();
			m5_a.Id.ShouldBe("model5_a");
			m5_a.Edge1.ShouldBeNull();
			m5_a.Edge2.ShouldBeNull();
			m5_a.Edge3.ShouldBeNull();

			var m5_b = GetModel("model5_b", list);
			m5_b.ShouldNotBeNull();
			m5_b.Id.ShouldBe("model5_b");

			m5_b.Edge1.ShouldBeNull();
			m5_b.Edge2.ShouldBeNull();
			m5_b.Edge3.ShouldBeNull();
		}

		private ModelWith3Connections GetModel(string id, List<object> list)
		{
			var modelWith3ConnectionsList = list.Select(x => (ModelWith3Connections)x).ToList();
			return modelWith3ConnectionsList.SingleOrDefault(x => x.Id == id);
		}

		[Fact]
		public async Task TwoLevelQueryWorks()
		{
			var selectValue = new SelectValue
			{
				FieldName = "Edge1",
				SelectValues = new List<SelectValue>
				{
					new SelectValue { FieldName = "Id" },
					new SelectValue { FieldName = "Field1" }
				}
			};

			var list = PrepData();
			var qs = PrepQuery(selectValue);

			const string other1Id = "other1";

			var connectionEdges = new List<ConnectionEdge>
			{
				SetupConnectionEdge<ModelWith3ConnectionsEdge, ModelWith3Connections>(
					new ModelWith3ConnectionsEdge
					{
						Id = "other1edge",
						Field1 = "SHARP"
					}, "model5_a", other1Id)
			};

			SetupConnectionEdgeRepository(new string[] { "model5_a", "model5_b" }, connectionEdges, other1Id,
				new List<object> { new ModelWith3ConnectionsOther
				{
					Id = other1Id, Field1 = "testother"
				} });

			await _connectionEdgeHandler.QueryAsync(list, qs, null, null, 
				new List<ConnectionEdgeDestinationFilter>());

			list.Count.ShouldBe(2);

			var m5_a = GetModel("model5_a", list);
			m5_a.ShouldNotBeNull();
			m5_a.Id.ShouldBe("model5_a");

			m5_a.Edge1.ShouldNotBeNull();
			m5_a.Edge1.Id.ShouldBe("other1edge");
			m5_a.Edge1.Field1.ShouldBe("SHARP");

			m5_a.Edge1.Other.ShouldNotBeNull();
			m5_a.Edge1.Other.Id.ShouldBe("other1");
			m5_a.Edge1.Other.Field1.ShouldBe("testother");

			m5_a.Edge2.ShouldBeNull();
			m5_a.Edge3.ShouldBeNull();

			var m5_b = GetModel("model5_b", list);
			m5_b.ShouldNotBeNull();
			m5_b.Id.ShouldBe("model5_b");

			m5_b.Edge1.ShouldBeNull();
			m5_b.Edge2.ShouldBeNull();
			m5_b.Edge3.ShouldBeNull();
		}

		private ConnectionEdge SetupConnectionEdge<TMeta, TSrc>(TMeta meta,
			string srcId,
			string destId)
		{
			return new ConnectionEdge
			{
				DestinationId = destId,
				MetaFieldName = "Other",
				MetaType = typeof(TMeta).AssemblyQualifiedName,
				MetaValue = JsonConvert.SerializeObject(meta),
				SourceFieldName = "Edge1",
				SourceId = srcId,
				SourceType = typeof(TSrc).AssemblyQualifiedName
			};
		}

		private void SetupConnectionEdgeRepository(string[] idList,
			List<ConnectionEdge> connectionEdges, string connectionOtherId, List<object> connectionsOthers)
		{
			_connectionEdgeRepository.QueryAsync<ConnectionEdge>(Arg.Any<string>(),
				Arg.Is<IEnumerable<QueryParameter>>(x =>
				x.First().ContextValue.Comparison == Comparisons.Equal &&
				x.First().ContextValue.Values.Count == idList.Length &&
				x.First().ContextValue.Values.All(v => idList.Contains((string)v))), null, null)
				.Returns(Task.FromResult(connectionEdges.AsEnumerable()));

			_graphQlRepositoryProvider.QueryAsync(Arg.Any<string>(),
				Arg.Is<QueryStep>(x =>
				x.QueryParameters.First().ContextValue.Comparison == Comparisons.Equal &&
				x.QueryParameters.First().ContextValue.Values.Count == 1 &&
				x.QueryParameters.First().ContextValue.Values.First().Equals(connectionOtherId)), null)
				.Returns(Task.FromResult(connectionsOthers.AsEnumerable()));
		}
	}
}
