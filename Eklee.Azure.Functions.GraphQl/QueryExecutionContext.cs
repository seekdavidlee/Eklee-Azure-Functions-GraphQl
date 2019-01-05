using System.Collections.Generic;
using System.Linq;

namespace Eklee.Azure.Functions.GraphQl
{
	public class QueryExecutionContext
	{
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

		public Dictionary<string, object> Items = new Dictionary<string, object>();

		public List<TItem> GetItems<TItem>(string key)
		{
			return (List<TItem>)Items[key];
		}

		public List<object> ConvertItemsToObjectList(string key)
		{
			var obj = Items[key];

			if (obj.GetType().IsGenericType && obj.GetType().GetGenericTypeDefinition() == typeof(List<>))
			{
				dynamic objList = obj;
				if (objList.Count > 0)
				{
					return (List<object>)obj;
				}
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
