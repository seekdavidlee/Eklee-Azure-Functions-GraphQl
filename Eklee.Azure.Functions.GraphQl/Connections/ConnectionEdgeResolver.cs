using FastMember;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Eklee.Azure.Functions.GraphQl.Connections
{
	public class ConnectionEdgeResolver : IConnectionEdgeResolver
	{
		public List<ConnectionEdge> HandleConnectionEdges<TSource>(TSource item, Action<object> entityAction)
		{
			var internalConnectionEdgeState = new InternalConnectionEdgeState(entityAction);
			var connectionEdges = new List<ConnectionEdge>();
			var srcType = item.GetType();
			var srcTypeAccessor = TypeAccessor.Create(srcType);
			var id = srcTypeAccessor.GetKey(item);

			DiscoverConnectionEdges(srcTypeAccessor, id, srcType, item, connectionEdges, internalConnectionEdgeState);

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
				var connMembers = srcTypeAccessor.GetMembers().Where(x => x.GetAttribute(
					 typeof(ConnectionAttribute), false) as ConnectionAttribute != null).ToList();

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

		private void DiscoverConnectionEdges(TypeAccessor sourceTypeAccessor,
		string sourceId,
		Type sourceType,
		object instance,
		List<ConnectionEdge> connectionEdges,
		InternalConnectionEdgeState internalConnectionEdgeState)
		{
			foreach (var member in sourceTypeAccessor.GetMembers())
			{
				ConnectionAttribute connAttribute = member.GetAttribute(typeof(ConnectionAttribute), true) as ConnectionAttribute;
				if (connAttribute != null)
				{
					var value = sourceTypeAccessor[instance, member.Name];
					if (value != null)
					{
						HandleConnectionEdge(sourceId, sourceType, connAttribute, member, value, connectionEdges, internalConnectionEdgeState);
						sourceTypeAccessor[instance, member.Name] = null;
					}
				}
			}
		}

		private void HandleConnectionEdge(
			string sourceId,
			Type sourceType,
			ConnectionAttribute connAttribute,
			Member member,
			object edgeObjectInstance,
			List<ConnectionEdge> connectionEdges,
			InternalConnectionEdgeState internalConnectionEdgeState)
		{
			var edgeType = edgeObjectInstance.GetType();
			var edgeTypeAccessor = TypeAccessor.Create(edgeType);

			var connectionEdge = new ConnectionEdge
			{
				SourceType = sourceType.AssemblyQualifiedName,
				SourceFieldName = member.Name,
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
				DiscoverConnectionEdges(edgeTypeAccessor, id, edgeType, destObject, connectionEdges, internalConnectionEdgeState);
				internalConnectionEdgeState.InvokeAction(destObject, id, destinationModel.Type.Name);
				edgeTypeAccessor[edgeObjectInstance, destinationModel.Name] = null;
			}

			connectionEdge.DestinationId = destId.ToString();
			connectionEdge.Id = $"{connectionEdge.SourceFieldName}_{connectionEdge.SourceId}_{connectionEdge.DestinationId}";
			connectionEdge.MetaFieldName = destinationModel.Name;
			connectionEdge.MetaValue = JsonConvert.SerializeObject(edgeObjectInstance);
			connectionEdges.Add(connectionEdge);
		}
	}
}
