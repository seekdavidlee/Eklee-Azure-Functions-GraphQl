using Eklee.Azure.Functions.GraphQl.Repository;
using FastMember;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Connections
{
	public class ConnectionEdgeHandler : IConnectionEdgeHandler
	{
		private readonly IGraphQlRepositoryProvider _graphQlRepositoryProvider;
		private readonly IConnectionEdgeResolver _connectionEdgeResolver;
		private IGraphQlRepository _connectionEdgeRepository;

		public ConnectionEdgeHandler(
			IGraphQlRepositoryProvider graphQlRepositoryProvider,
			IConnectionEdgeResolver connectionEdgeResolver)
		{
			_graphQlRepositoryProvider = graphQlRepositoryProvider;
			_connectionEdgeResolver = connectionEdgeResolver;
		}

		public async Task QueryAsync(List<object> results, QueryStep queryStep, IGraphRequestContext graphRequestContext)
		{
			var selections = GetFirstComplexSelectValues(queryStep);

			if (selections != null && selections.Count > 0)
			{
				await QueryAndPopulateEdgeConnections(selections, results, graphRequestContext);
			}
		}

		private const string ConnectionEdgeQueryName = "ConnectionEdgeQueryName";
		private const string EntityQueryName = "EntityQueryName";

		private async Task QueryAndPopulateEdgeConnections(
			List<SelectValue> selections,
			List<object> results,
			IGraphRequestContext graphRequestContext)
		{
			var edgeQueryParameters = _connectionEdgeResolver.ListConnectionEdgeQueryParameter(results)
				.Where(x => selections.Any(y => y.FieldName.ToLower() == x.SourceFieldName.ToLower())).ToList();

			if (edgeQueryParameters.Count > 0)
			{
				var connectionEdges = (await GetConnectionEdgeRepository().QueryAsync<ConnectionEdge>(
					ConnectionEdgeQueryName,
					edgeQueryParameters.ToQueryParameters(), null, graphRequestContext)).ToList();

				if (connectionEdges.Count > 0)
				{
					var accessor = TypeAccessor.Create(results.First().GetType());
					var dictionary = new Dictionary<string, object>();
					results.ForEach(item => dictionary.Add(accessor.GetKey(item), item));

					foreach (var connectionEdge in connectionEdges)
					{
						var sourceObject = dictionary[connectionEdge.SourceId];

						var edgeObject = DeserializeObject(connectionEdge.MetaValue, connectionEdge.MetaType);

						accessor[sourceObject, connectionEdge.SourceFieldName] = edgeObject;

						var selection = selections.SingleOrDefault(s => s.FieldName.ToLower() == connectionEdge.SourceFieldName.ToLower());

						if (selection != null)
						{
							var entity = (await _graphQlRepositoryProvider.QueryAsync(EntityQueryName,
								CreateQueryStep(connectionEdge),
								graphRequestContext)).SingleOrDefault();

							if (entity == null) continue;

							var edgeObjectTypeAccessor = TypeAccessor.Create(edgeObject.GetType());
							edgeObjectTypeAccessor[edgeObject, connectionEdge.MetaFieldName] = entity;

							await QueryAndPopulateEdgeConnections(new List<SelectValue> { selection }, new List<object> { entity }, graphRequestContext);
						}
					}
				}
			}
		}

		private QueryStep CreateQueryStep(ConnectionEdge connectionEdge)
		{
			var qp = new QueryStep();
			var destType = Type.GetType(connectionEdge.SourceType);
			var destTypeAccessor = TypeAccessor.Create(destType);
			var destQueryMember = destTypeAccessor.GetMembers().Single(m => m.Name.ToLower() == connectionEdge.DestinationFieldName.ToLower());
			qp.QueryParameters.Add(new QueryParameter
			{
				ContextValue = new ContextValue { Comparison = Comparisons.Equal, Values = new List<object> { connectionEdge.DestinationId } },
				MemberModel = new ModelMember(destType, destTypeAccessor, destQueryMember, false)
			});
			return qp;
		}

		private IGraphQlRepository GetConnectionEdgeRepository()
		{
			if (_connectionEdgeRepository == null)
				_connectionEdgeRepository = _graphQlRepositoryProvider.GetRepository<ConnectionEdge>();
			return _connectionEdgeRepository;
		}

		private static MethodInfo _jsonConvertDeserializeObject = typeof(JsonConvert).GetMethods().Single(x =>
				x.Name == "DeserializeObject" &&
				x.IsGenericMethod &&
				x.GetParameters().Count() == 1);

		private static object DeserializeObject(string value, string typeName)
		{
			MethodInfo generic = _jsonConvertDeserializeObject.MakeGenericMethod(
				Type.GetType(typeName));

			return generic.Invoke(null, new object[] { value });
		}

		private List<SelectValue> GetFirstComplexSelectValues(QueryStep queryStep)
		{
			var qp = queryStep.QueryParameters.FirstOrDefault();
			if (qp != null)
			{
				var selectedValues = qp.ContextValue.SelectValues;

				if (selectedValues != null)
					return selectedValues.Where(x => x.SelectValues != null && x.SelectValues.Count > 0).ToList();
			}

			return null;
		}
	}
}
