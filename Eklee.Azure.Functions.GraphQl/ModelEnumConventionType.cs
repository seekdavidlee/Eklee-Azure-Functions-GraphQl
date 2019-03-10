using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using FastMember;
using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl
{
	public class ModelEnumConventionType<T> : EnumerationGraphType
	{
		public ModelEnumConventionType()
		{
			var type = typeof(T);
			Name = type.Name.ToLower();

			if (type.GetCustomAttribute(typeof(DescriptionAttribute), false) is DescriptionAttribute desc)
			{
				Description = desc.Description;
			}

			foreach (int val in Enum.GetValues(type))
			{
				var memInfo = type.GetMember(type.GetEnumName(val))[0];
				var descriptionAttribute = memInfo
					.GetCustomAttributes(typeof(DescriptionAttribute), false)
					.FirstOrDefault() as DescriptionAttribute;

				AddValue(memInfo.Name.ToLower(),
					descriptionAttribute != null ? descriptionAttribute.Description : "", val);
			}
		}
	}
}
