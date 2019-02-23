using GraphQL;
using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl.Example.TestSearch
{
	public class TestSearchSchema : Schema
	{
		public TestSearchSchema(IDependencyResolver resolver, TestSearchQuery query, TestSearchMutation mutation) :
			base(resolver)
		{
			Query = query;
			Mutation = mutation;
		}
	}
}
