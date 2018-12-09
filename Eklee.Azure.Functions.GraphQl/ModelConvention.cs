using GraphQL;
using GraphQL.Types;
using System;
using System.Collections.Generic;

namespace Eklee.Azure.Functions.GraphQl
{
	public class ModelConvention<TSourceType>
	{
		public ModelConvention()
		{
			Name = ModelType.Name.ToLower();
		}

		public string Name { get; }

		public ModelType<TSourceType> ModelType { get; } = new ModelType<TSourceType>();

		public void ForEachWithField(Action<Type, string, string> addFieldAction)
		{
			ModelType.ForEach(m =>
			{
				// See: https://graphql-dotnet.github.io/docs/getting-started/schema-types

				if (m.Type == typeof(string) ||
					m.Type == typeof(int) ||
					m.Type == typeof(long) ||
					m.Type == typeof(bool) ||
					m.Type == typeof(double) ||
					m.Type == typeof(List<string>))
				{
					addFieldAction(m.Type.GetGraphTypeFromType(), m.Name, m.GetDescription());
				}
				else
				{
					// See: https://docs.microsoft.com/en-us/dotnet/framework/reflection-and-codedom/how-to-examine-and-instantiate-generic-types-with-reflection

					if (m.Type.IsGenericType && m.Type.GetGenericTypeDefinition() == typeof(List<>))
					{
						addFieldAction(typeof(ListGraphType<>).MakeGenericType(typeof(ModelConventionType<>).MakeGenericType(m.Type.GetGenericArguments()[0])), m.Name, m.GetDescription());
					}
					else
					{
						addFieldAction(typeof(ModelConventionType<>).MakeGenericType(m.Type), m.Name, m.GetDescription());
					}
				}
			});
		}
	}
}
