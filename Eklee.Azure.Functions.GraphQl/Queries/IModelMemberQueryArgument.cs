using GraphQL.Builders;
using GraphQL.Types;
using System.Collections.Generic;

namespace Eklee.Azure.Functions.GraphQl.Queries
{
	public interface IModelMemberQueryArgument
	{
		bool CanHandle(ModelMember modelMember);

		IEnumerable<QueryArgument> GetArguments(ModelMember modelMember);

		IEnumerable<ConnectionBuilder<object>> GetConnectionBuilderArguments(
			ModelMember modelMember,
			ConnectionBuilder<object> connectionBuilder);
	}
}
