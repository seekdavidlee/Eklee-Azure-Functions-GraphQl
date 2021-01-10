using System.Collections.Generic;
using Eklee.Azure.Functions.GraphQl.Filters;
using Eklee.Azure.Functions.GraphQl.Repository.Search;
using GraphQL.Builders;
using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl.Queries
{
	public class StringQueryArgument : IModelMemberQueryArgument
	{
		public bool CanHandle(ModelMember modelMember)
		{
			return modelMember.SourceType != typeof(SearchModel) && modelMember.IsString;
		}

		private QueryArgument GetArgument(ModelMember modelMember)
		{
			return modelMember.IsOptional ?
				(QueryArgument)new QueryArgument<ModelConventionInputType<StringFilter>>
				{
					Name = modelMember.Name,
					Description = modelMember.Description
				} :
				new QueryArgument<NonNullGraphType<ModelConventionInputType<StringFilter>>>
				{
					Name = modelMember.Name,
					Description = modelMember.Description
				};
		}

		public IEnumerable<QueryArgument> GetArguments(ModelMember modelMember)
		{
			return new List<QueryArgument> { GetArgument(modelMember) };
		}

		public ConnectionBuilder<object> GetConnectionBuilderArgument(ModelMember modelMember, ConnectionBuilder<object> connectionBuilder)
		{
			return modelMember.IsOptional ?
						connectionBuilder.Argument<ModelConventionInputType<StringFilter>>(modelMember.Name, modelMember.Description) :
						connectionBuilder.Argument<NonNullGraphType<ModelConventionInputType<StringFilter>>>(modelMember.Name, modelMember.Description);
		}

		public IEnumerable<ConnectionBuilder<object>> GetConnectionBuilderArguments(
			ModelMember modelMember,
			ConnectionBuilder<object> connectionBuilder)
		{
			return new List<ConnectionBuilder<object>> { GetConnectionBuilderArgument(modelMember, connectionBuilder) };
		}
	}
}
