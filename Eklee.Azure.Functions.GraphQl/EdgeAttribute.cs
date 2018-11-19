using System;

namespace Eklee.Azure.Functions.GraphQl
{
	public class EdgeAttribute : Attribute
	{
		private readonly Type _entityType;

		public EdgeAttribute(Type entityType)
		{
			_entityType = entityType;
		}
	}
}
