using Microsoft.Azure.Cosmos.Table;

namespace Eklee.Azure.Functions.GraphQl.Repository.TableStorage
{
	public class TableStorageComparisonInt : TableStorageComparisonBase<int>
	{
		protected override string GenerateFilterConditionFor(Comparisons comparison, int value)
		{
			if (QueryParameter.ContextValue.Comparison == Comparisons.Equal)
				// ReSharper disable once PossibleInvalidOperationException
				return TableQuery.GenerateFilterConditionForInt(QueryParameter.MemberModel.Member.Name, QueryComparisons.Equal, value);

			if (QueryParameter.ContextValue.Comparison == Comparisons.GreaterEqualThan)
				// ReSharper disable once PossibleInvalidOperationException
				return TableQuery.GenerateFilterConditionForInt(QueryParameter.MemberModel.Member.Name, QueryComparisons.GreaterThanOrEqual, value);

			if (QueryParameter.ContextValue.Comparison == Comparisons.GreaterThan)
				// ReSharper disable once PossibleInvalidOperationException
				return TableQuery.GenerateFilterConditionForInt(QueryParameter.MemberModel.Member.Name, QueryComparisons.GreaterThan, value);

			if (QueryParameter.ContextValue.Comparison == Comparisons.LessEqualThan)
				// ReSharper disable once PossibleInvalidOperationException
				return TableQuery.GenerateFilterConditionForInt(QueryParameter.MemberModel.Member.Name, QueryComparisons.LessThanOrEqual, value);

			if (QueryParameter.ContextValue.Comparison == Comparisons.LessThan)
				// ReSharper disable once PossibleInvalidOperationException
				return TableQuery.GenerateFilterConditionForInt(QueryParameter.MemberModel.Member.Name, QueryComparisons.LessThan, value);

			if (QueryParameter.ContextValue.Comparison == Comparisons.NotEqual)
				// ReSharper disable once PossibleInvalidOperationException
				return TableQuery.GenerateFilterConditionForInt(QueryParameter.MemberModel.Member.Name, QueryComparisons.NotEqual, value);

			return null;
		}

		protected override bool AssertCanHandleContextValue(object o)
		{
			return o is int value && value != 0;
		}
	}
}
