﻿using Microsoft.Azure.Cosmos.Table;

namespace Eklee.Azure.Functions.GraphQl.Repository.TableStorage
{
	public class TableStorageComparisonBool : ITableStorageComparison
	{
		private QueryParameter _queryParameter;

		private bool? _value;

		public bool CanHandle(QueryParameter queryParameter)
		{
			_queryParameter = queryParameter;
			_value = null;

			if (queryParameter.ContextValue.IsSingleValue() &&
			    queryParameter.ContextValue.GetFirstValue() is bool value)
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
				return TableQuery.GenerateFilterConditionForBool(_queryParameter.MemberModel.Member.Name, QueryComparisons.Equal, _value.Value);

			return null;
		}
	}
}
