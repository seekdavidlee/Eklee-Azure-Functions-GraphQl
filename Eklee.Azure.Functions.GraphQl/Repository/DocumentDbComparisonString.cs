using System;

namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public class DocumentDbComparisonString
	{
		private readonly QueryParameter _queryParameter;

		public DocumentDbComparisonString(QueryParameter queryParameter)
		{
			_queryParameter = queryParameter;
		}

		public override string ToString()
		{
			var fieldName = _queryParameter.MemberModel.Member.Name;
			var fieldValue = _queryParameter.ContextValue.Value;

			if (fieldValue is string strFieldValue)
			{
				string comparison;

				switch (_queryParameter.Comparison)
				{
					case Comparisons.StringContains:
						comparison = "CONTAINS";
						break;

					case Comparisons.StringStartsWith:
						comparison = "STARTSWITH";
						break;

					case Comparisons.StringEndsWith:
						comparison = "ENDSWITH";
						break;

					default:
						throw new NotImplementedException($"Comparison {_queryParameter.Comparison} is not implemented.");
				}

				return $" {comparison}(x.{fieldName}, '{strFieldValue}')";
			}

			throw new InvalidOperationException("It does not appear the value is a string type. Hence, this comparison is not valid.");
		}
	}
}
