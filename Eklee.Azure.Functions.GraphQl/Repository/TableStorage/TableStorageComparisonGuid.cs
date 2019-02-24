using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace Eklee.Azure.Functions.GraphQl.Repository.TableStorage
{
	public class TableStorageComparisonGuid : TableStorageComparisonBase<Guid>
	{
		protected override string GenerateFilterConditionFor(Comparisons comparison, Guid value)
		{
			if (QueryParameter.ContextValue.Comparison == Comparisons.Equal)
				// ReSharper disable once PossibleInvalidOperationException
				return TableQuery.GenerateFilterConditionForGuid(QueryParameter.MemberModel.Member.Name, QueryComparisons.Equal, value);

			return null;
		}

		protected override bool AssertCanHandleContextValue(object o)
		{
			return o is Guid;
		}
	}
}
