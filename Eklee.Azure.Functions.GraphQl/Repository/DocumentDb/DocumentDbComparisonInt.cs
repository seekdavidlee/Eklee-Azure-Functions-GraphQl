namespace Eklee.Azure.Functions.GraphQl.Repository.DocumentDb
{
	public class DocumentDbComparisonInt : BaseDocumentDbComparison<int>
	{
		protected override bool AssertContextValue(int value)
		{
			return value != 0;
		}

		protected override string GetComprisonString(Comparisons comparison, string[] names)
		{
			if (comparison == Comparisons.Equal)
			{
				if (names.Length == 1)
				{
					return $" {GetPropertyName()} = {names[0]}";
				}

				if (names.Length > 1)
				{
					return $" {GetPropertyName()} in ({string.Join(",", names)})";
				}
			}

			if (names.Length == 1)
			{
				var name = names[0];

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
