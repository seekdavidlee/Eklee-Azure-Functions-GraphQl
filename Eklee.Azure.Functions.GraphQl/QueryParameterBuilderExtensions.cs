using System;
using System.Collections.Generic;
using GraphQL.Builders;
using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl
{
	public static class QueryParameterBuilderExtensions
	{
		public static QueryArguments GetQueryArguments<TSource>(this QueryParameterBuilder<TSource> queryParameterBuilder)
		{
			var queryArguments = new List<QueryArgument>();

			queryParameterBuilder.ForEach((modelMember, m) =>
			{
				if (m.Type == typeof(string))
				{
					if (modelMember.IsOptional)
					{
						queryArguments.Add(new QueryArgument<StringGraphType>
						{
							Name = m.Name,
							Description = m.GetDescription()
						});
					}
					else
					{
						queryArguments.Add(new QueryArgument<NonNullGraphType<StringGraphType>>
						{
							Name = m.Name,
							Description = m.GetDescription()
						});
					}

					return;
				}

				throw new NotImplementedException();
			});


			return new QueryArguments(queryArguments);
		}

		public static void PopulateWithArguments<TSource>(this QueryParameterBuilder<TSource> queryParameterBuilder,
			ConnectionBuilder<ModelConventionType<TSource>, object> connectionBuilder)
		{
			queryParameterBuilder.ForEach((modelMember, m) =>
			{
				if (m.Type == typeof(string))
					connectionBuilder = modelMember.IsOptional ?
						connectionBuilder.Argument<StringGraphType>(modelMember.Name, m.GetDescription()) :
						connectionBuilder.Argument<NonNullGraphType<StringGraphType>>(modelMember.Name, m.GetDescription());
			});
		}
	}
}
