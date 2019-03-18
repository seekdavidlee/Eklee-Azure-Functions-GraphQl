namespace Eklee.Azure.Functions.GraphQl.Repository.DocumentDb
{
	public class DocumentDbComparisonString : BaseDocumentDbComparison<string>
	{
		protected override bool AssertContextValue(string value)
		{
			return !string.IsNullOrEmpty(value);
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

				switch (comparison)
				{
					case Comparisons.StringContains:
						return $" CONTAINS({GetPropertyName()}, {name})";

					case Comparisons.StringStartsWith:
						return $" STARTSWITH({GetPropertyName()}, {name})";

					case Comparisons.StringEndsWith:
						return $" ENDSWITH({GetPropertyName()}, {name})";

					default:
						return null;
				}
			}

			return null;
		}
	}
}
