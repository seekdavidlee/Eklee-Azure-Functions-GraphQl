using GraphQL.Types;
using System;

namespace Eklee.Azure.Functions.GraphQl.Example.TestDocumentDbWithPermissions
{
	public class TestDocumentDbWithPermissionsSchema : Schema
	{
		public TestDocumentDbWithPermissionsSchema(IServiceProvider resolver, TestDocumentDbWithPermissionsQuery query, TestDocumentDbWithPermissionsMutation mutation) :
			base(resolver)
		{
			Query = query;
			Mutation = mutation;
		}
	}
}