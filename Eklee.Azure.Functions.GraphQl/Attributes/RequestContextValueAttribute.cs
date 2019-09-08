using System;

namespace Eklee.Azure.Functions.GraphQl.Attributes
{
	public class RequestContextValueAttribute : Attribute
	{
		public RequestContextValueAttribute(Type type)
		{
			Type = type;
		}

		public Type Type { get; }
	}
}
