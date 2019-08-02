namespace Eklee.Azure.Functions.GraphQl
{
	public class MapperQueryExecutionContext
	{
		public MapperQueryExecutionContext(QueryExecutionContext context, QueryStep queryStep)
		{
			Context = context;
			QueryStep = queryStep;
		}

		public QueryExecutionContext Context { get; }
		public QueryStep QueryStep { get; }
	}
}