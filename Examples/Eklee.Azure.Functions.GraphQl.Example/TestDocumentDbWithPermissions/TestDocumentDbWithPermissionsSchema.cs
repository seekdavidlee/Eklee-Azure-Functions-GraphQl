using GraphQL;
using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl.Example.TestDocumentDbWithPermissions
{
	public class TestDocumentDbWithPermissionsSchema : Schema
	{
		public TestDocumentDbWithPermissionsSchema(IDependencyResolver resolver, TestDocumentDbWithPermissionsQuery query, TestDocumentDbWithPermissionsMutation mutation) :
			base(resolver)
		{
			Query = query;
			Mutation = mutation;
		}
	}
}