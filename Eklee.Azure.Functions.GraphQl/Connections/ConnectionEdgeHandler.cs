using Eklee.Azure.Functions.GraphQl.Repository;
using FastMember;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Connections
{
	public class ConnectionEdgeHandler : IConnectionEdgeHandler, IMutationPreAction
	{
		private readonly IGraphQlRepositoryProvider _graphQlRepositoryProvider;
		private readonly IConnectionEdgeResolver _connectionEdgeResolver;
		private readonly ILogger _logger;
		private IGraphQlRepository _connectionEdgeRepository;

		public ConnectionEdgeHandler(
			IGraphQlRepositoryProvider graphQlRepositoryProvider,
			IConnectionEdgeResolver connectionEdgeResolver,
			ILogger logger)
		{
			_graphQlRepositoryProvider = graphQlRepositoryProvider;
			_connectionEdgeResolver = connectionEdgeResolver;
			_logger = logger;
		}

		public async Task RemoveEdgeConnections(object item, IGraphRequestContext graphRequestContext)
		{
			if (!IsRepositoryExist()) return;

			var edgeQueryParameters = _connectionEdgeResolver.ListConnectionEdgeQueryParameter(new List<object> { item });

			var connectionEdges = (await GetConnectionEdgeRepository().QueryAsync<ConnectionEdge>(
				ConnectionEdgeQueryName,
				edgeQueryParameters.ToQueryParameters(), null, graphRequestContext)).ToList();

			if (edgeQueryParameters.Count > 0)
			{
				foreach (var connectionEdge in connectionEdges)
				{
					await GetConnectionEdgeRepository().DeleteAsync(connectionEdge, graphRequestContext);
				}
			}
		}

		public async Task QueryAsync(List<object> results, QueryStep queryStep, IGraphRequestContext graphRequestContext,
			QueryExecutionContext queryExecutionContext,
			List<ConnectionEdgeDestinationFilter> connectionEdgeDestinationFilters)
		{
			var selections = GetFirstComplexSelectValues(queryStep);

			if (selections != null && selections.Count > 0)
			{
				await QueryAndPopulateEdgeConnections(selections, results, graphRequestContext, connectionEdgeDestinationFilters, queryExecutionContext);
			}
		}

		private const string ConnectionEdgeQueryName = "ConnectionEdgeQueryName";
		private const string EntityQueryName = "EntityQueryName";

		private async Task QueryAndPopulateEdgeConnections(
			List<SelectValue> selections,
			List<object> results,
			IGraphRequestContext graphRequestContext,
			List<ConnectionEdgeDestinationFilter> connectionEdgeDestinationFilters,
			QueryExecutionContext queryExecutionContext)
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
						if (!dictionary.ContainsKey(connectionEdge.SourceId))
							throw new InvalidOperationException($"{connectionEdge.SourceId} is invalid.");

						var sourceObject = dictionary[connectionEdge.SourceId];

						var edgeObject = DeserializeObject(connectionEdge.MetaValue, connectionEdge.MetaType);

						var member = accessor.GetMembers().Single(x => x.Name == connectionEdge.SourceFieldName);
						if (member.IsList())
						{
							member.CreateNewListIfNullThenAddItemToList(accessor, sourceObject, edgeObject);
						}
						else
						{
							accessor[sourceObject, connectionEdge.SourceFieldName] = edgeObject;
						}

						var selection = selections.SingleOrDefault(s => s.FieldName.ToLower() == connectionEdge.SourceFieldName.ToLower());

						if (selection != null)
						{
							var edgeObjectType = edgeObject.GetType();
							var edgeObjectTypeAccessor = TypeAccessor.Create(edgeObjectType);
							var entity = await GetValue(edgeObjectTypeAccessor, connectionEdge, graphRequestContext,
								connectionEdgeDestinationFilters,
								queryExecutionContext);

							if (entity == null)
							{
								_logger.LogWarning($"The following connection edge did not yield a record: {JsonConvert.SerializeObject(connectionEdge)}");
								continue;
							}

							edgeObjectTypeAccessor[edgeObject, connectionEdge.MetaFieldName] = entity;

							var matchingFieldName = connectionEdge.MetaFieldName.ToLower();
							var matchingSelections = selections.Where(x => x.SelectValues.Any(y => y.FieldName.ToLower() == matchingFieldName));

							foreach (var matchingSelection in matchingSelections)
							{
								var selectionWithSelects = matchingSelection.SelectValues.Single(x => x.FieldName.ToLower() == matchingFieldName)
									.SelectValues.Where(x => x.SelectValues.Count > 0).ToList();

								if (selectionWithSelects.Count > 0)
									await QueryAndPopulateEdgeConnections(selectionWithSelects, new List<object> { entity }, graphRequestContext,
										connectionEdgeDestinationFilters, queryExecutionContext);
							}
						}
					}
				}
			}
		}

		private const string MultipleEntitiesDetectedError =
			"Unable to determine the correct destination entity due to multiple entities sharing the same key value. This may be due to the use of partitioning. Please implement ForDestinationFilter<T>(expression) in the query builder to help filter down to a single result.";

		private async Task<object> GetValue(
			TypeAccessor edgeObjectTypeAccessor,
			ConnectionEdge connectionEdge,
			IGraphRequestContext graphRequestContext,
			List<ConnectionEdgeDestinationFilter> connectionEdgeDestinationFilters,
			QueryExecutionContext queryExecutionContext)
		{
			var member = edgeObjectTypeAccessor.GetMembers().Single(x => x.Name == connectionEdge.MetaFieldName);
			var destTypeAccessor = TypeAccessor.Create(member.Type);
			var destQueryMember = destTypeAccessor.GetMembers().Single(m => m.GetAttribute(typeof(KeyAttribute), true) != null);
			var qp = new QueryStep();
			qp.QueryParameters.Add(new QueryParameter
			{
				ContextValue = new ContextValue { Comparison = Comparisons.Equal, Values = new List<object> { connectionEdge.DestinationId } },
				MemberModel = new ModelMember(member.Type, destTypeAccessor, destQueryMember, false)
			});

			if (connectionEdgeDestinationFilters != null)
			{
				connectionEdgeDestinationFilters.Where(x => x.Type == member.Type.AssemblyQualifiedName)
					.ToList().ForEach(connectionEdgeDestinationFilter =>
					{
						qp.QueryParameters.Add(new QueryParameter
						{
							ContextValue = new ContextValue { Comparison = Comparisons.Equal, Values = connectionEdgeDestinationFilter.Mapper(queryExecutionContext) },
							MemberModel = connectionEdgeDestinationFilter.ModelMember
						});
					});
			}

			var entities = (await _graphQlRepositoryProvider.QueryAsync(EntityQueryName, qp, graphRequestContext)).ToList();

			if (entities.Count > 1) throw new InvalidOperationException(MultipleEntitiesDetectedError);

			return entities.SingleOrDefault();
		}

		private bool? _isRepositoryExist;
		private bool IsRepositoryExist()
		{
			if (!_isRepositoryExist.HasValue)
			{
				_isRepositoryExist = _graphQlRepositoryProvider.IsRepositoryExist<ConnectionEdge>();
			}

			return _isRepositoryExist.Value;
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

		public int ExecutionOrder => 0;

		private static object DeserializeObject(string value, string typeName)
		{
			MethodInfo generic = _jsonConvertDeserializeObject.MakeGenericMethod(
				Type.GetType(typeName));

			return generic.Invoke(null, new object[] { value });
		}

		private List<SelectValue> GetFirstComplexSelectValues(QueryStep queryStep)
		{
			var qp = queryStep.QueryParameters.FirstOrDefault();
			if (qp != null && qp.ContextValue != null)
			{
				var selectedValues = qp.ContextValue.SelectValues;

				if (selectedValues != null)
					return selectedValues.Where(x => x.SelectValues != null && x.SelectValues.Count > 0).ToList();
			}

			return null;
		}

		public async Task DeleteAllEdgeConnectionsOfType<T>(IGraphRequestContext graphRequestContext)
		{
			if (!IsRepositoryExist()) return;

			var connectionEdges = (await GetConnectionEdgeRepository().QueryAsync<ConnectionEdge>(
				ConnectionEdgeQueryName,
				GetQueryParameters<T>(), null, graphRequestContext)).ToList();

			if (connectionEdges.Count > 0)
			{
				foreach (var connectionEdge in connectionEdges)
				{
					await GetConnectionEdgeRepository().DeleteAsync(connectionEdge, graphRequestContext);
				}
			}
		}

		private List<QueryParameter> GetQueryParameters<T>()
		{
			var accessor = TypeAccessor.Create(typeof(ConnectionEdge));
			var member = accessor.GetMembers().Single(x => x.Name == "SourceType");

			return new List<QueryParameter> {new QueryParameter
			{
				ContextValue = new ContextValue { Comparison = Comparisons.Equal, Values = new List<object> { typeof(T).AssemblyQualifiedName } },
				MemberModel = new ModelMember(member.Type, accessor, member, false)
			} };
		}

		public async Task TryHandlePreItem<TSource>(MutationActionItem<TSource> mutationActionItem)
		{
			switch (mutationActionItem.Action)
			{
				case MutationActions.Delete:
					await RemoveEdgeConnections(mutationActionItem.Item, mutationActionItem.RequestContext);
					break;

				case MutationActions.DeleteAll:
					await DeleteAllEdgeConnectionsOfType<TSource>(mutationActionItem.RequestContext);
					break;

				default:
					// Do nothing. We are not handling.
					break;
			}
		}
	}
}
