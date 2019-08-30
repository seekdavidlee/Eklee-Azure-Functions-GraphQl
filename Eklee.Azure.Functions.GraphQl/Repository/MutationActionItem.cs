using System.Collections.Generic;

namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public class MutationActionItem<TSource>
	{
		public TSource Item { get; set; }
		public IGraphRequestContext RequestContext { get; set; }
		public MutationActions Action { get; set; }
		public IGraphQlRepositoryProvider RepositoryProvider { get; set; }
		public object ObjectItem { get; set; }
		public List<TSource> Items { get; set; }
		public List<object> ObjectItems { get; set; }
	}
}
