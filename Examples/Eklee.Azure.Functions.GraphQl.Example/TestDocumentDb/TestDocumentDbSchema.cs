using GraphQL.Types;
using System;

namespace Eklee.Azure.Functions.GraphQl.Example.TestDocumentDb
{
	public class TestDocumentDbSchema : Schema
	{
		public TestDocumentDbSchema(IServiceProvider resolver, TestDocumentDbQuery query, TestDocumentDbMutation mutation) :
			base(resolver)
		{
			Query = query;
			Mutation = mutation;
		}
	}
}
