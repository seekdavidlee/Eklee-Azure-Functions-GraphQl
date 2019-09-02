using FastMember;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Eklee.Azure.Functions.GraphQl.Connections
{
	public class ConnectionEdgeResolver : IConnectionEdgeResolver
	{
		public List<ConnectionEdge> HandleConnectionEdges<TSource>(List<TSource> items, Action<object> entityAction)
		{
			var connectionEdges = new List<ConnectionEdge>();

			foreach (var item in items)
			{
				connectionEdges.AddRange(HandleConnectionEdges(item, entityAction));
			}

			return connectionEdges;
		}

		public List<ConnectionEdge> HandleConnectionEdges<TSource>(TSource item, Action<object> entityAction)
		{
			var internalConnectionEdgeState = new InternalConnectionEdgeState(entityAction);
			var connectionEdges = new List<ConnectionEdge>();
			var srcType = item.GetType();
			var srcTypeAccessor = TypeAccessor.Create(srcType);
			var id = srcTypeAccessor.GetKey(item);

			DiscoverConnectionEdges(id, item, connectionEdges, internalConnectionEdgeState, entityAction);

			internalConnectionEdgeState.InvokeAction(item, id, srcType.Name);

			return connectionEdges;
		}

		public List<ConnectionEdgeQueryParameter> ListConnectionEdgeQueryParameter(IEnumerable<object> items)
		{
			var itemList = items.ToList();
			if (itemList.Count > 0)
			{
				var srcType = itemList.First().GetType();
				var srcTypeAccessor = TypeAccessor.Create(srcType);
				var connMembers = srcTypeAccessor.GetMembers().Where(x => GetConnectionAttribute(x) != null).ToList();

				var list = new List<ConnectionEdgeQueryParameter>();
				itemList.ForEach(item =>
				{
					var srcId = srcTypeAccessor.GetKey(item);
					list.AddRange(connMembers.Select(cm =>
						new ConnectionEdgeQueryParameter
						{
							SourceFieldName = cm.Name,
							SourceType = srcType.AssemblyQualifiedName,
							SourceId = srcId
						}).ToList());
				});

				return list;
			}

			return null;
		}

		private ConnectionAttribute GetConnectionAttribute(Member member)
		{
			return member.GetAttribute(
					 typeof(ConnectionAttribute), false) as ConnectionAttribute;
		}

		private void DiscoverConnectionEdges(
			string sourceId,
			object instance,
			List<ConnectionEdge> connectionEdges,
			InternalConnectionEdgeState internalConnectionEdgeState, Action<object> entityAction)
		{
			Type sourceType = instance.GetType();
			var sourceTypeAccessor = TypeAccessor.Create(sourceType);

			foreach (var member in sourceTypeAccessor.GetMembers())
			{
				if (GetConnectionAttribute(member) != null)
				{
					var value = sourceTypeAccessor[instance, member.Name];
					if (value != null)
					{
						if (member.IsList())
						{
							var argType = member.Type.GetGenericArguments()[0];
							foreach (var itemValue in (IList)value)
							{
								HandleConnectionEdge(sourceId, sourceType,
									argType,
									member.Name, itemValue, connectionEdges,
									internalConnectionEdgeState, entityAction, true);
							}
						}
						else
						{
							HandleConnectionEdge(sourceId, sourceType,
								member.Type, member.Name, value, connectionEdges,
								internalConnectionEdgeState, entityAction, false);
						}

						sourceTypeAccessor[instance, member.Name] = null;
					}
				}
			}
		}

		private void HandleConnectionEdge(
			string sourceId,
			Type sourceType,
			Type edgeType,
			string sourceFieldName,
			object edgeObjectInstance,
			List<ConnectionEdge> connectionEdges,
			InternalConnectionEdgeState internalConnectionEdgeState,
			Action<object> entityAction,
			bool isListEdge)
		{
			var edgeTypeAccessor = TypeAccessor.Create(edgeType);

			var connectionEdge = new ConnectionEdge
			{
				SourceType = sourceType.AssemblyQualifiedName,
				SourceFieldName = sourceFieldName,
				SourceId = sourceId,
				MetaType = edgeType.AssemblyQualifiedName
			};

			Member destinationId = null;
			Member destinationModel = null;

			foreach (var edgeMember in edgeTypeAccessor.GetMembers())
			{
				var connectionEdgeDestinationKey = edgeMember.GetAttribute(typeof(ConnectionEdgeDestinationKeyAttribute),
					false) as ConnectionEdgeDestinationKeyAttribute;

				if (connectionEdgeDestinationKey != null)
				{
					destinationId = edgeMember;
				}
				else
				{
					var connectionEdgeDestination = edgeMember.GetAttribute(typeof(ConnectionEdgeDestinationAttribute),
						false) as ConnectionEdgeDestinationAttribute;

					if (connectionEdgeDestination != null)
					{
						destinationModel = edgeMember;
					}
				}
			}

			if (destinationId == null)
				throw new InvalidProgramException("Property with ConnectionEdgeDestinationKeyAttribute is required on a Connection.");

			if (destinationModel == null)
				throw new InvalidProgramException("Property with ConnectionEdgeDestinationAttribute is required on a Connection.");

			var destId = edgeTypeAccessor[edgeObjectInstance, destinationId.Name];

			if (destId == null)
				throw new InvalidProgramException("Value on property with ConnectionEdgeDestinationKeyAttribute is required!");

			// Eval destination model instance first.
			var destObject = edgeTypeAccessor[edgeObjectInstance, destinationModel.Name];

			if (destObject != null)
			{
				var id = destId.ToString();
				DiscoverConnectionEdges(id, destObject, connectionEdges, internalConnectionEdgeState, entityAction);
				internalConnectionEdgeState.InvokeAction(destObject, id, destinationModel.Type.Name);
				edgeTypeAccessor[edgeObjectInstance, destinationModel.Name] = null;
			}

			connectionEdge.DestinationId = destId.ToString();
			connectionEdge.DestinationFieldName = destinationId.Name;

			connectionEdge.Id = isListEdge ?
				$"{connectionEdge.SourceFieldName}_src{connectionEdge.SourceId}_des{connectionEdge.DestinationId}" :
				$"{connectionEdge.SourceFieldName}_{connectionEdge.SourceId}";

			connectionEdge.MetaFieldName = destinationModel.Name;
			connectionEdge.MetaValue = JsonConvert.SerializeObject(edgeObjectInstance);
			connectionEdges.Add(connectionEdge);
		}
	}
}
