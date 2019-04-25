using Eklee.Azure.Functions.GraphQl.Connections;
using Eklee.Azure.Functions.GraphQl.Repository;
using Eklee.Azure.Functions.GraphQl.Tests.Models;
using FastMember;
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
		private readonly Type _type = typeof(ModelWith3Connections);

		public ConnectionEdgeHandlerTests()
		{
			_graphQlRepositoryProvider = Substitute.For<IGraphQlRepositoryProvider>();
			_connectionEdgeRepository = Substitute.For<IGraphQlRepository>();

			_graphQlRepositoryProvider.GetRepository<ConnectionEdge>().Returns(_connectionEdgeRepository);

			_connectionEdgeHandler = new ConnectionEdgeHandler(_graphQlRepositoryProvider, new ConnectionEdgeResolver());
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

		private QueryStep PrepQuery()
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
			return qs;
		}

		[Fact]
		public async Task OneLevelQueryWorks()
		{
			var list = PrepData();
			var qs = PrepQuery();

			await _connectionEdgeHandler.QueryAsync(list, qs, null);

			list.Count.ShouldBe(2);

			var m5_a = GetModel("model5_a", list);
			m5_a.ShouldNotBeNull();
			m5_a.Id.ShouldBe("model5_a");

			var m5_b = GetModel("model5_b", list);
			m5_b.ShouldNotBeNull();
			m5_b.Id.ShouldBe("model5_b");
		}

		private ModelWith3Connections GetModel(string id, List<object> list)
		{
			var modelWith3ConnectionsList = list.Select(x => (ModelWith3Connections)x).ToList();
			return modelWith3ConnectionsList.SingleOrDefault(x => x.Id == id);
		}

		[Fact]
		public async Task TwoLevelQueryWorks()
		{
			var list = PrepData();
			var qs = PrepQuery();
			qs.QueryParameters.First().ContextValue.SelectValues.Add(new SelectValue
			{
				FieldName = "Edge1",
				SelectValues = new List<SelectValue>
				{
					new SelectValue { FieldName = "Id" },
					new SelectValue { FieldName = "Field1" }
				}
			});

			SetupConnectionEdgeInputAndOutput(new string[] { "model5_a", "model5_b" },
				SetupConnectionEdge<ModelWith3ConnectionsEdge, ModelWith3Connections>(
					new ModelWith3ConnectionsEdge
					{
						 
					}, "model5_a", "id", "model5_a_conn"));

			await _connectionEdgeHandler.QueryAsync(list, qs, null);

			list.Count.ShouldBe(2);

			var m5_a = GetModel("model5_a", list);
			m5_a.ShouldNotBeNull();
			m5_a.Id.ShouldBe("model5_a");

			var m5_b = GetModel("model5_b", list);
			m5_b.ShouldNotBeNull();
			m5_b.Id.ShouldBe("model5_b");
		}

		private ConnectionEdge SetupConnectionEdge<TMeta, TSrc>(TMeta meta,
			string srcId,
			string destFieldName,
			string destId)
		{
			return new ConnectionEdge
			{
				DestinationFieldName = destFieldName,
				DestinationId = destId,
				MetaFieldName = "Edge1",
				MetaType = typeof(TMeta).FullName,
				MetaValue = JsonConvert.SerializeObject(meta),
				SourceFieldName = "id",
				SourceId = srcId,
				SourceType = typeof(TSrc).FullName
			};
		}

		private void SetupConnectionEdgeInputAndOutput(string[] idList, params ConnectionEdge[] connectionEdges)
		{
			_connectionEdgeRepository.QueryAsync<ConnectionEdge>(Arg.Any<string>(),
				Arg.Is<IEnumerable<QueryParameter>>(x =>
				x.First().ContextValue.Comparison == Comparisons.StringContains &&
				x.First().ContextValue.Values.Count == idList.Length &&
				x.First().ContextValue.Values.All(v => idList.Contains((string)v))), null, null)
				.Returns(Task.FromResult(connectionEdges.AsEnumerable()));
		}
	}
}
