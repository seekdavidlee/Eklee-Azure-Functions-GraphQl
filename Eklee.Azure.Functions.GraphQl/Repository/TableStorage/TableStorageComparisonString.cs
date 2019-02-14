using Microsoft.WindowsAzure.Storage.Table;

namespace Eklee.Azure.Functions.GraphQl.Repository.TableStorage
{
	public class TableStorageComparisonString : ITableStorageComparison
	{
		private QueryParameter _queryParameter;

		private string _value;
		public bool CanHandle(QueryParameter queryParameter)
		{
			_queryParameter = queryParameter;
			_value = null;

			if (queryParameter.ContextValue.Value is string value && !string.IsNullOrEmpty(value))
			{
				_value = value;
				return true;
			}

			return false;
		}

		public string Generate()
		{
			if (_queryParameter.ContextValue.Comparison == Comparisons.Equal)
			{
				return TableQuery.GenerateFilterCondition(_queryParameter.MemberModel.Member.Name, QueryComparisons.Equal, _value);
			}

			return null;
		}
	}
}
