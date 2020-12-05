using GraphQL.Types;
using System;

namespace Eklee.Azure.Functions.GraphQl.Example.TestSearch
{
	public class TestSearchSchema : Schema
	{
		public TestSearchSchema(IServiceProvider resolver, TestSearchQuery query, TestSearchMutation mutation) :
			base(resolver)
		{
			Query = query;
			Mutation = mutation;
		}
	}
}
