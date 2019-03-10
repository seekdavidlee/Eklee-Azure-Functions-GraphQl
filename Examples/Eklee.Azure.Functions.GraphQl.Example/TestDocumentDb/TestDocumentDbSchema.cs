using GraphQL;
using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl.Example.TestDocumentDb
{
	public class TestDocumentDbSchema : Schema
	{
		public TestDocumentDbSchema(IDependencyResolver resolver, TestDocumentDbQuery query, TestDocumentDbMutation mutation) :
			base(resolver)
		{
			Query = query;
			Mutation = mutation;
		}
	}
}
