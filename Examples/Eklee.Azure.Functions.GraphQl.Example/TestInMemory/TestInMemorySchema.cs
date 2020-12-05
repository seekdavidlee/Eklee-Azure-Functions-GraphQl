using GraphQL.Types;
using System;

namespace Eklee.Azure.Functions.GraphQl.Example.TestInMemory
{
	public class TestInMemorySchema : Schema
	{
		public TestInMemorySchema(IServiceProvider resolver, TestInMemoryQuery query, TestInMemoryMutation mutation) :
			base(resolver)
		{
			Query = query;
			Mutation = mutation;
		}
	}
}
