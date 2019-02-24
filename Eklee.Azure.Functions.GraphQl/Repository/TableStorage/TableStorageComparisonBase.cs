using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;

namespace Eklee.Azure.Functions.GraphQl.Repository.TableStorage
{
	public abstract class TableStorageComparisonBase<T> : ITableStorageComparison
	{
		protected T Value;
		protected T[] Values;
		protected QueryParameter QueryParameter;

		protected abstract string GenerateFilterConditionFor(Comparisons comparison, T value);
		protected abstract bool AssertCanHandleContextValue(object o);

		public bool CanHandle(QueryParameter queryParameter)
		{
			QueryParameter = queryParameter;
			Value = default(T);

			var value = QueryParameter.ContextValue.GetFirstValue();
			if (AssertCanHandleContextValue(value))
			{
				if (QueryParameter.ContextValue.IsSingleValue())
					Value = (T)value;
				else
					Values = QueryParameter.ContextValue.Values.Select(x => (T)x).ToArray();

				return true;
			}

			return false;
		}

		public string Generate()
		{
			if (!QueryParameter.ContextValue.Comparison.HasValue) return null;

			Comparisons comparison = QueryParameter.ContextValue.Comparison.Value;
			if (QueryParameter.ContextValue.IsSingleValue())
				return GenerateFilterConditionFor(comparison, Value);

			string filters = null;
			Values.ToList().ForEach(value =>
			{
				var current = GenerateFilterConditionFor(comparison, value);

				if (current == null) return;

				if (filters == null)
				{
					filters = current;
				}
				else
				{
					filters = TableQuery.CombineFilters(filters, TableOperators.Or, current);
				}
			});

			return filters;
		}
	}
}
