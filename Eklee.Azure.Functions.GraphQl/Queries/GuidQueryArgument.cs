using Eklee.Azure.Functions.GraphQl.Filters;
using GraphQL.Builders;
using GraphQL.Types;
using System.Collections.Generic;

namespace Eklee.Azure.Functions.GraphQl.Queries
{
	public class GuidQueryArgument : IModelMemberQueryArgument
	{
		public bool CanHandle(ModelMember modelMember)
		{
			return modelMember.IsGuid;
		}

		private QueryArgument GetArgument(ModelMember modelMember)
		{
			return modelMember.IsOptional ?
				(QueryArgument)new QueryArgument<ModelConventionInputType<GuidFilter>>
				{
					Name = modelMember.Name,
					Description = modelMember.Description
				} :
				new QueryArgument<NonNullGraphType<ModelConventionInputType<GuidFilter>>>
				{
					Name = modelMember.Name,
					Description = modelMember.Description
				};
		}

		private ConnectionBuilder<ModelConventionType<TSource>> GetConnectionBuilderArgument<TSource>(ModelMember modelMember, ConnectionBuilder<ModelConventionType<TSource>> connectionBuilder)
		{
			return modelMember.IsOptional ?
						connectionBuilder.Argument<ModelConventionInputType<GuidFilter>>(modelMember.Name, modelMember.Description) :
						connectionBuilder.Argument<NonNullGraphType<ModelConventionInputType<GuidFilter>>>(modelMember.Name, modelMember.Description);
		}

		public IEnumerable<QueryArgument> GetArguments(ModelMember modelMember)
		{
			return new List<QueryArgument> { GetArgument(modelMember) };
		}

		public IEnumerable<ConnectionBuilder<ModelConventionType<TSource>>> GetConnectionBuilderArguments<TSource>(
			ModelMember modelMember,
			ConnectionBuilder<ModelConventionType<TSource>> connectionBuilder)
		{
			return new List<ConnectionBuilder<ModelConventionType<TSource>>> { GetConnectionBuilderArgument(modelMember, connectionBuilder) };
		}
	}
}
