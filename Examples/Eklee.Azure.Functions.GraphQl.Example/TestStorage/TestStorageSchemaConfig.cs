using GraphQL;
using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl.Example.TestStorage.Core
{
	public class TestStorageSchemaConfig : Schema
	{
		public TestStorageSchemaConfig(IDependencyResolver resolver, 
			TestStorageQueryConfigObjectGraphType query, 
			TestStorageMutationObjectGraphType mutation) : base(resolver)
		{
			Query = query;
			Mutation = mutation;
		}
	}
}
