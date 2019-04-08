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
			Dictionary<string, ConnectionEdge> connectionEdges = new Dictionary<string, ConnectionEdge>();

			var srcType = TypeAccessor.Create(typeof(TSource));
			var id = srcType.GetKey(item);

			connectionEdges.Add(id, new ConnectionEdge());

			DiscoverConnectionEdges(srcType, id, item, connectionEdges, entityAction);

			entityAction(item);

			var list = connectionEdges.Values.Where(x => !string.IsNullOrEmpty(x.SourceId)).ToList();

			list.ForEach(val =>
			{
				val.Id = $"{val.FieldName}_{val.SourceId}_{val.DestinationId}";
			});

			return list;
		}

		private void DiscoverConnectionEdges(TypeAccessor type,
			string sourceId,
			object instance,
			Dictionary<string, ConnectionEdge> connectionEdges,
			Action<object> entityAction)
		{
			foreach (var member in type.GetMembers())
			{
				ConnectionAttribute connAttribute = member.GetAttribute(typeof(ConnectionAttribute), true) as ConnectionAttribute;
				if (connAttribute != null)
				{
					var child = type[instance, member.Name];

					if (child != null)
					{
						var childType = TypeAccessor.Create(child.GetType());
						var id = childType.GetKey(child);

						if (!connectionEdges.ContainsKey(id))
						{
							connectionEdges.Add(id, new ConnectionEdge
							{
								FieldName = member.Name,
								SourceId = sourceId,
								DestinationId = id
							});

							DiscoverConnectionEdges(childType, id, child, connectionEdges, entityAction);

							type[instance, member.Name] = null;

							entityAction?.Invoke(child);
						}
						else
						{
							type[instance, member.Name] = null;
						}
					}
				}
			}

			foreach (var member in type.GetMembers())
			{
				ConnectionMetaForAttribute connMetaForAttribute = member.GetAttribute(typeof(ConnectionMetaForAttribute), true) as ConnectionMetaForAttribute;
				if (connMetaForAttribute != null)
				{
					var connEdge = connectionEdges.Values.SingleOrDefault(value => value.FieldName == connMetaForAttribute.PropertyName);
					if (connEdge != null)
					{
						connEdge.MetaFieldName = member.Name;
						connEdge.MetaType = member.Type.FullName;
						connEdge.MetaValue = JsonConvert.SerializeObject(type[instance, member.Name]);

						type[instance, member.Name] = null;
					}
				}
			}
		}
	}
}
