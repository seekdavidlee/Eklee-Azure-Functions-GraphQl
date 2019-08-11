using System.Collections.Generic;
using Eklee.Azure.Functions.GraphQl.Filters;
using Eklee.Azure.Functions.GraphQl.Repository.Search;
using GraphQL.Builders;
using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl.Queries
{
	public class SearchModelQueryArgument : IModelMemberQueryArgument
	{
		public bool CanHandle(ModelMember modelMember)
		{
			return modelMember.SourceType == typeof(SearchModel) && modelMember.IsString;
		}

		public QueryArgument GetArgument(ModelMember modelMember)
		{
			return modelMember.IsOptional ?

				new QueryArgument<ModelConventionInputType<StringEqualFilter>>
				{
					Name = modelMember.Name,
					Description = modelMember.Description
				} :

				(QueryArgument)new QueryArgument<NonNullGraphType<ModelConventionInputType<StringEqualFilter>>>
				{
					Name = modelMember.Name,
					Description = modelMember.Description
				};
		}

		public IEnumerable<QueryArgument> GetArguments(ModelMember modelMember)
		{
			return new List<QueryArgument> { GetArgument(modelMember) };
		}

		public ConnectionBuilder<ModelConventionType<TSource>, object> GetConnectionBuilderArgument<TSource>(
			ModelMember modelMember,
			ConnectionBuilder<ModelConventionType<TSource>, object> connectionBuilder)
		{
			return modelMember.IsOptional ?
						connectionBuilder.Argument<ModelConventionInputType<StringEqualFilter>>(modelMember.Name, modelMember.Description) :
						connectionBuilder.Argument<NonNullGraphType<ModelConventionInputType<StringEqualFilter>>>(modelMember.Name, modelMember.Description);
		}

		public IEnumerable<ConnectionBuilder<ModelConventionType<TSource>, object>> GetConnectionBuilderArguments<TSource>(
			ModelMember modelMember,
			ConnectionBuilder<ModelConventionType<TSource>, object> connectionBuilder)
		{
			return new List<ConnectionBuilder<ModelConventionType<TSource>, object>> { GetConnectionBuilderArgument(modelMember, connectionBuilder) };
		}
	}
}
