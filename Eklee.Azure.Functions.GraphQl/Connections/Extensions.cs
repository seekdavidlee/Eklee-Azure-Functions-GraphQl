using FastMember;
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

		public static bool IsList(this Member member)
		{
			return member.Type.IsGenericType && member.Type.GetGenericTypeDefinition() == typeof(List<>);
		}

		public static void CreateNewListIfNullThenAddItemToList(this Member member, TypeAccessor accessor, object sourceObject, object item)
		{
			if (accessor[sourceObject, member.Name] == null)
			{
				accessor[sourceObject, member.Name] = Activator.CreateInstance(member.Type);
			}

			member.Type.GetMethod("Add").Invoke(accessor[sourceObject, member.Name], new object[] { item });
		}
	}
}
