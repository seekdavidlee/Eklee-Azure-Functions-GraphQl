using GraphQL;
using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl.Example.TestSearch2
{
	public class TestSearchSchema2 : Schema
	{
		public TestSearchSchema2(IDependencyResolver resolver, TestSearchQuery2 query, TestSearchMutation2 mutation) :
			base(resolver)
		{
			Query = query;
			Mutation = mutation;
		}
	}
}
