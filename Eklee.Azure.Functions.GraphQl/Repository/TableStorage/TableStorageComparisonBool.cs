using Microsoft.WindowsAzure.Storage.Table;

namespace Eklee.Azure.Functions.GraphQl.Repository.TableStorage
{
	public class TableStorageComparisonBool : ITableStorageComparison
	{
		private QueryParameter _queryParameter;

		private int? _value;

		public bool CanHandle(QueryParameter queryParameter)
		{
			_queryParameter = queryParameter;
			_value = null;

			if (queryParameter.ContextValue.Value is int value && value != 0)
			{
				_value = value;
				return true;
			}

			return false;
		}

		public string Generate()
		{
			if (_queryParameter.ContextValue.Comparison == Comparisons.Equal)
				// ReSharper disable once PossibleInvalidOperationException
				return TableQuery.GenerateFilterConditionForInt(_queryParameter.MemberModel.Member.Name, QueryComparisons.Equal, _value.Value);

			if (_queryParameter.ContextValue.Comparison == Comparisons.GreaterEqualThan)
				// ReSharper disable once PossibleInvalidOperationException
				return TableQuery.GenerateFilterConditionForInt(_queryParameter.MemberModel.Member.Name, QueryComparisons.GreaterThanOrEqual, _value.Value);

			if (_queryParameter.ContextValue.Comparison == Comparisons.GreaterThan)
				// ReSharper disable once PossibleInvalidOperationException
				return TableQuery.GenerateFilterConditionForInt(_queryParameter.MemberModel.Member.Name, QueryComparisons.GreaterThan, _value.Value);

			if (_queryParameter.ContextValue.Comparison == Comparisons.LessEqualThan)
				// ReSharper disable once PossibleInvalidOperationException
				return TableQuery.GenerateFilterConditionForInt(_queryParameter.MemberModel.Member.Name, QueryComparisons.LessThanOrEqual, _value.Value);

			if (_queryParameter.ContextValue.Comparison == Comparisons.LessThan)
				// ReSharper disable once PossibleInvalidOperationException
				return TableQuery.GenerateFilterConditionForInt(_queryParameter.MemberModel.Member.Name, QueryComparisons.LessThan, _value.Value);

			if (_queryParameter.ContextValue.Comparison == Comparisons.NotEqual)
				// ReSharper disable once PossibleInvalidOperationException
				return TableQuery.GenerateFilterConditionForInt(_queryParameter.MemberModel.Member.Name, QueryComparisons.NotEqual, _value.Value);

			return null;
		}
	}
}
