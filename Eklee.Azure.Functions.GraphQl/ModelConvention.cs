using GraphQL;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Eklee.Azure.Functions.GraphQl.Attributes;
using FastMember;

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

		private Type GetGraphTypeFromTypeWithModelFieldAttribute(Member member)
		{
			if (member.Type == typeof(Guid))
			{
				if (member.GetAttribute(typeof(KeyAttribute), false) is KeyAttribute)
					return typeof(IdGraphType);

				if (member.GetAttribute(typeof(ModelFieldAttribute), false) is ModelFieldAttribute guidModelField)
				{
					return guidModelField.IsRequired
						? typeof(NonNullGraphType<StringGraphType>)
						: typeof(StringGraphType);
				}

				return typeof(StringGraphType);
			}

			if (member.GetAttribute(typeof(ModelFieldAttribute), false) is ModelFieldAttribute modelField)
			{
				return member.Type.GetGraphTypeFromType(!modelField.IsRequired);
			}

			return member.Type.GetGraphTypeFromType();
		}

		private Type GetEnumGraphType(Member m, Type baseType)
		{
			var enumType = typeof(ModelEnumConventionType<>).MakeGenericType(baseType);

			if (m.GetAttribute(typeof(ModelFieldAttribute), false) is ModelFieldAttribute modelField && modelField.IsRequired)
			{
				var nonNull = typeof(NonNullGraphType<>).MakeGenericType(enumType);
				return nonNull;
			}

			return enumType;
		}

		public void ForEachWithField(Action<Type, string, string, Func<ResolveFieldContext<TSourceType>, object>> addFieldAction, bool isInput)
		{
			ModelType.ForEach(m =>
			{
				// See: https://graphql-dotnet.github.io/docs/getting-started/schema-types

				if (m.Type == typeof(string) ||
					m.Type == typeof(int) ||
					m.Type == typeof(long) ||
					m.Type == typeof(bool) ||
					m.Type == typeof(double) ||
					m.Type == typeof(DateTime) ||
					m.Type == typeof(Guid) ||
					m.Type == typeof(List<string>))
				{
					addFieldAction(GetGraphTypeFromTypeWithModelFieldAttribute(m),
						m.Name, m.GetDescription(), null);
				}
				else
				{
					if (m.Type.IsEnum)
					{
						addFieldAction(GetEnumGraphType(m, m.Type), m.Name, m.GetDescription(), null);
						return;
					}

					if (m.Type.IsNullable() &&
						m.Type.GetGenericArguments().Length > 0)
					{
						var nullableType = m.Type.GetGenericArguments()[0];

						if (nullableType.IsEnum)
						{
							addFieldAction(GetEnumGraphType(m, nullableType), m.Name, m.GetDescription(), null);
							return;
						}
					}

					// See: https://docs.microsoft.com/en-us/dotnet/framework/reflection-and-codedom/how-to-examine-and-instantiate-generic-types-with-reflection

					var genericType = isInput ? typeof(ModelConventionInputType<>) : typeof(ModelConventionType<>);

					if (m.Type.IsGenericType && m.Type.GetGenericTypeDefinition() == typeof(List<>))
					{
						addFieldAction(typeof(ListGraphType<>).MakeGenericType(genericType.MakeGenericType(m.Type.GetGenericArguments()[0])), m.Name, m.GetDescription(), null);
					}
					else
					{
						addFieldAction(genericType.MakeGenericType(m.Type), m.Name, m.GetDescription(), null);
					}
				}
			});
		}
	}
}
