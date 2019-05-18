using GraphQL.Types;
using System.Collections.Generic;

namespace Eklee.Azure.Functions.GraphQl
{
	public class QueryArgumentsBuilder : IQueryArgumentsBuilder
	{
		public QueryArgumentsBuilder()
		{

		}

		public QueryArguments BuildNonNull<T>(string sourceName)
		{
			var args = new List<QueryArgument>();
			args.Add(new QueryArgument<NonNullGraphType<ModelConventionInputType<T>>> { Name = sourceName });
			return new QueryArguments(args);
		}

		public QueryArguments BuildList<T>(string sourceName)
		{
			var args = new List<QueryArgument>();
			args.Add(new QueryArgument<ListGraphType<ModelConventionInputType<T>>> { Name = sourceName });
			return new QueryArguments(args);
		}
	}
}
