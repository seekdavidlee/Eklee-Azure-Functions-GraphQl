using FastMember;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Eklee.Azure.Functions.GraphQl.Connections
{
	public class ConnectionEdgeResolver : IConnectionEdgeResolver
	{
		public List<ConnectionEdge> HandleConnectionEdges<TSource>(TSource item, Action<object> entityAction)
		{
			var internalConnectionEdgeState = new InternalConnectionEdgeState(entityAction);
			var connectionEdges = new List<ConnectionEdge>();
			var type = item.GetType();
			var srcType = TypeAccessor.Create(type);
			var id = srcType.GetKey(item);

			DiscoverConnectionEdges(srcType, id, item, connectionEdges, internalConnectionEdgeState);

			internalConnectionEdgeState.InvokeAction(item, id, type.Name);

			return connectionEdges;
		}

		private void DiscoverConnectionEdges(TypeAccessor type,
			string sourceId,
			object instance,
			List<ConnectionEdge> connectionEdges,
			InternalConnectionEdgeState internalConnectionEdgeState)
		{
			foreach (var member in type.GetMembers())
			{
				ConnectionAttribute connAttribute = member.GetAttribute(typeof(ConnectionAttribute), true) as ConnectionAttribute;
				if (connAttribute != null)
				{
					var value = type[instance, member.Name];
					if (value != null)
					{
						HandleConnectionEdge(sourceId, connAttribute, member, value, connectionEdges, internalConnectionEdgeState);
						type[instance, member.Name] = null;
					}
				}
			}
		}

		private void HandleConnectionEdge(
			string sourceId,
			ConnectionAttribute connAttribute,
			Member member,
			object edgeObjectInstance,
			List<ConnectionEdge> connectionEdges,
			InternalConnectionEdgeState internalConnectionEdgeState)
		{
			var type = edgeObjectInstance.GetType();
			var edgeType = TypeAccessor.Create(type);

			var connectionEdge = new ConnectionEdge
			{
				FieldName = member.Name,
				SourceId = sourceId,
				MetaType = type.FullName
			};

			Member destinationId = null;
			Member destinationModel = null;

			foreach (var edgeMember in edgeType.GetMembers())
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

			var destId = edgeType[edgeObjectInstance, destinationId.Name];

			if (destId == null)
				throw new InvalidProgramException("Value on property with ConnectionEdgeDestinationKeyAttribute is required!");

			// Eval destination model instance first.
			var destObject = edgeType[edgeObjectInstance, destinationModel.Name];

			if (destObject != null)
			{
				var id = destId.ToString();
				DiscoverConnectionEdges(edgeType, id, destObject, connectionEdges, internalConnectionEdgeState);
				internalConnectionEdgeState.InvokeAction(destObject, id, destinationModel.Type.Name);
				edgeType[edgeObjectInstance, destinationModel.Name] = null;
			}

			connectionEdge.DestinationId = destId.ToString();
			connectionEdge.Id = $"{connectionEdge.FieldName}_{connectionEdge.SourceId}_{connectionEdge.DestinationId}";
			connectionEdge.MetaFieldName = destinationModel.Name;
			connectionEdge.MetaValue = JsonConvert.SerializeObject(edgeObjectInstance);
			connectionEdges.Add(connectionEdge);
		}
	}
}
