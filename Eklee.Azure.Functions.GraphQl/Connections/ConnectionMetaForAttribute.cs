using System;

namespace Eklee.Azure.Functions.GraphQl.Connections
{
	public class ConnectionMetaForAttribute : Attribute
	{
		public ConnectionMetaForAttribute(string propertyName)
		{
			PropertyName = propertyName;
		}

		public string PropertyName { get; }
	}
}
