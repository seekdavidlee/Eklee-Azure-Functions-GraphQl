using System;

namespace Eklee.Azure.Functions.GraphQl.Repository.DocumentDb
{
	public class DocumentDbComparisonDate : BaseDocumentDbComparison<DateTime>
	{
		protected override bool AssertContextValue(DateTime value)
		{
			return value != DateTime.MinValue;
		}

		protected override string GetComprisonString(Comparisons comparison, string[] names)
		{
			if (names.Length == 1)
			{
				var name = names[0];

				if (comparison == Comparisons.Equal)
					return $" {GetPropertyName()} = {name}";

				if (comparison == Comparisons.NotEqual)
					return $" {GetPropertyName()} != {name}";

				if (comparison == Comparisons.GreaterThan)
					return $" {GetPropertyName()} > {name}";

				if (comparison == Comparisons.GreaterEqualThan)
					return $" {GetPropertyName()} >= {name}";

				if (comparison == Comparisons.LessThan)
					return $" {GetPropertyName()} < {name}";

				if (comparison == Comparisons.LessEqualThan)
					return $" {GetPropertyName()} <= {name}";
			}

			return null;
		}
	}
}
