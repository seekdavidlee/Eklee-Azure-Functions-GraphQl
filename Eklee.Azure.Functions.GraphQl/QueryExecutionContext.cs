using System.Collections.Generic;
using System.Linq;

namespace Eklee.Azure.Functions.GraphQl
{
	public class QueryExecutionContext
	{
		public QueryExecutionContext(IGraphRequestContext graphRequestContext)
		{
			RequestContext = graphRequestContext;
		}

		private List<object> _queryResults;
		private List<object> _results;

		internal void SetQueryResult(List<object> items)
		{
			_queryResults = items;
		}

		public List<TItem> GetQueryResults<TItem>()
		{
			return _queryResults.Select(x => (TItem)x).ToList();
		}

		/// <summary>
		/// Consumers may use items to store their custom objects across each context processing steps.
		/// </summary>
		public Dictionary<string, object> Items = new Dictionary<string, object>();

		/// <summary>
		/// System items are specific entities created as a result of internal process.
		/// </summary>
		internal Dictionary<string, object> SystemItems = new Dictionary<string, object>();

		/// <summary>
		/// Current request context.
		/// </summary>
		public IGraphRequestContext RequestContext { get; }

		public List<TItem> GetSystemItems<TItem>()
		{
			return (List<TItem>)SystemItems[typeof(TItem).FullName];
		}

		public List<TItem> GetItems<TItem>(string key)
		{
			return (List<TItem>)Items[key];
		}

		public List<object> ConvertItemsToObjectList<T>(string key)
		{
			if (Items[key] is List<T> objList)
			{
				return objList.Select(x => (object)x).ToList();
			}

			return new List<object>();
		}

		public void SetResults<TResult>(List<TResult> results)
		{
			_results = results.Select(x => (object)x).ToList();
		}

		public List<TResult> GetResults<TResult>()
		{
			return _results.Select(x => (TResult)x).ToList();
		}
	}
}
