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

			queryParameterBuilder.ForEach(modelMember =>
			{
				if (modelMember.PathMember.Type == typeof(string))
				{
					if (modelMember.IsOptional)
					{
						queryArguments.Add(new QueryArgument<StringGraphType>
						{
							Name = modelMember.Name,
							Description = modelMember.Description
						});
					}
					else
					{
						queryArguments.Add(new QueryArgument<NonNullGraphType<StringGraphType>>
						{
							Name = modelMember.Name,
							Description = modelMember.Description
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
			queryParameterBuilder.ForEach(modelMember =>
			{
				if (modelMember.PathMember.Type == typeof(string))
					connectionBuilder = modelMember.IsOptional ?
						connectionBuilder.Argument<StringGraphType>(modelMember.Name, modelMember.Description) :
						connectionBuilder.Argument<NonNullGraphType<StringGraphType>>(modelMember.Name, modelMember.Description);
			});
		}
	}
}
