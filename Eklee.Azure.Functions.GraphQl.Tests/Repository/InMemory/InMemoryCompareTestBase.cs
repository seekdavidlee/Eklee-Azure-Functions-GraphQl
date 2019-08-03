using FastMember;
using System.Collections.Generic;
using System.Linq;

namespace Eklee.Azure.Functions.GraphQl.Tests.Repository.InMemory
{
	public abstract class InMemoryCompareTestBase
	{
		private ModelMember GetModelMember(string name)
		{
			var type = typeof(InMemItem);
			var ta = TypeAccessor.Create(type);
			return new ModelMember(type, ta, ta.GetMembers().Single(x => x.Name == name), false);
		}

		protected QueryParameter GetQueryParameter(string name, object value, Comparisons comparisons)
		{
			return new QueryParameter
			{
				ContextValue = new ContextValue
				{
					Comparison = comparisons,
					Values = new List<object> { value }
				},
				MemberModel = GetModelMember(name)
			};
		}
	}
}
