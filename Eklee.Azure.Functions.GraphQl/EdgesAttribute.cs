using System;

namespace Eklee.Azure.Functions.GraphQl
{
	public class EdgesAttribute : Attribute
	{
		private readonly Type _entityType;

		public EdgesAttribute(Type entityType)
		{
			_entityType = entityType;
		}
	}
}