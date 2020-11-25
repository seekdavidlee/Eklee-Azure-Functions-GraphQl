using Microsoft.Azure.Cosmos.Table;

namespace Eklee.Azure.Functions.GraphQl.Repository.TableStorage
{
	public class TableStorageComparisonString : TableStorageComparisonBase<string>
	{
		protected override string GenerateFilterConditionFor(Comparisons comparison, string value)
		{
			if (comparison == Comparisons.Equal)
			{
				return TableQuery.GenerateFilterCondition(QueryParameter.MemberModel.Member.Name,
					QueryComparisons.Equal, value);
			}

			return null;
		}

		protected override bool AssertCanHandleContextValue(object o)
		{
			return o is string value && !string.IsNullOrEmpty(value);
		}
	}
}
