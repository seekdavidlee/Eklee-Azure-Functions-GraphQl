using GraphQL.Types;
using System;

namespace Eklee.Azure.Functions.GraphQl.Example.TestStorage.Core
{
	public class TestStorageSchemaConfig : Schema
	{
		public TestStorageSchemaConfig(IServiceProvider resolver,
			TestStorageQueryConfigObjectGraphType query,
			TestStorageMutationObjectGraphType mutation) : base(resolver)
		{
			Query = query;
			Mutation = mutation;
		}
	}
}
