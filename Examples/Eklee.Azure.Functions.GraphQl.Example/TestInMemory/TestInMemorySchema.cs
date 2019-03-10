using GraphQL;
using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl.Example.TestInMemory
{
	public class TestInMemorySchema : Schema
	{
		public TestInMemorySchema(IDependencyResolver resolver, TestInMemoryQuery query, TestInMemoryMutation mutation) :
			base(resolver)
		{
			Query = query;
			Mutation = mutation;
		}
	}
}
