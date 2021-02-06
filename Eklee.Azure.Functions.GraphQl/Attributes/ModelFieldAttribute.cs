using System;

namespace Eklee.Azure.Functions.GraphQl.Attributes
{
	public class ModelFieldAttribute : Attribute
	{
		public bool IsRequired { get; }
		public bool UseNullWhenOptional { get; }

		public ModelFieldAttribute(bool isRequired, bool useNullWhenOptional = false)
		{
			IsRequired = isRequired;
			UseNullWhenOptional = useNullWhenOptional;
		}
	}
}
