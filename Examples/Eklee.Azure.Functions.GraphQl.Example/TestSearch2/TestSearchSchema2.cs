using GraphQL.Types;
using System;

namespace Eklee.Azure.Functions.GraphQl.Example.TestSearch2
{
	public class TestSearchSchema2 : Schema
	{
		public TestSearchSchema2(IServiceProvider resolver, TestSearchQuery2 query, TestSearchMutation2 mutation) :
			base(resolver)
		{
			Query = query;
			Mutation = mutation;
		}
	}
}
