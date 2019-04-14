using FastMember;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Eklee.Azure.Functions.GraphQl.Connections
{
	public static class Extensions
	{
		public static List<QueryParameter> ToQueryParameters(this List<ConnectionEdgeQueryParameter> connectionEdgeQueryParameters)
		{
			var srcType = typeof(ConnectionEdge);
			var connEdgeTypeAccessor = TypeAccessor.Create(srcType);
			var members = connEdgeTypeAccessor.GetMembers().ToList();

			var srcIdQp = new QueryParameter
			{
				ContextValue = new ContextValue
				{
					Comparison = Comparisons.StringContains,
					Values = connectionEdgeQueryParameters.Select(x => x.SourceId).Distinct().Select(x => (object)x).ToList()
				},
				MemberModel = new ModelMember(srcType, connEdgeTypeAccessor,
				members.Single(x => x.Name == "SourceId"), false)
			};

			var srcTypeQp = new QueryParameter
			{
				ContextValue = new ContextValue
				{
					Comparison = Comparisons.Equal,
					Values = new List<object> { connectionEdgeQueryParameters.First().SourceType }
				},
				MemberModel = new ModelMember(srcType, connEdgeTypeAccessor,
				members.Single(x => x.Name == "SourceType"), false)
			};

			return new List<QueryParameter> { srcIdQp, srcTypeQp };
		}

		public static void Populate(this IEnumerable<ConnectionEdge> connectionEdges, List<object> results)
		{
			var list = connectionEdges.ToList();

			if (list.Count > 0)
			{
				var accessor = TypeAccessor.Create(results.First().GetType());
				var dictionary = new Dictionary<string, object>();
				results.ForEach(item => dictionary.Add(accessor.GetKey(item), item));

				var methodInfo = typeof(JsonConvert).GetMethods().Single(x =>
				x.Name == "DeserializeObject" &&
				x.IsGenericMethod &&
				x.GetParameters().Count() == 1);

				list.ForEach(x =>
				{
					var o = dictionary[x.SourceId];

					MethodInfo generic = methodInfo.MakeGenericMethod(Type.GetType(x.MetaType));

					accessor[o, x.SourceFieldName] = generic.Invoke(null, new object[] { x.MetaValue });
				});
			}
		}
	}
}
