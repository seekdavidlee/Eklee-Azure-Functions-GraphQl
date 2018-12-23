using System;

namespace Eklee.Azure.Functions.GraphQl.Attributes
{
	public class ModelFieldAttribute : Attribute
	{
		public bool IsRequired { get; }

		public ModelFieldAttribute(bool isRequired)
		{
			IsRequired = isRequired;
		}
	}
}
