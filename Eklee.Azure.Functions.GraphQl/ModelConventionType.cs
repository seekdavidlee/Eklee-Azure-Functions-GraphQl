using System.ComponentModel;
using System.Linq;
using FastMember;
using GraphQL;
using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl
{
	public class ModelConventionType<TSourceType> : ObjectGraphType<TSourceType>
	{
		public ModelConventionType()
		{
			var type = typeof(TSourceType);
			var accessor = TypeAccessor.Create(type);

			Name = type.Name.ToLower();

			accessor.GetMembers().ToList().ForEach(m =>
				Field(m.Type.GetGraphTypeFromType(), m.Name,
					((DescriptionAttribute)m.GetAttribute(typeof(DescriptionAttribute), false)).Description));
		}
	}
}
