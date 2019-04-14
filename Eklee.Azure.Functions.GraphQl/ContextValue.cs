using System.Collections.Generic;
using System.Linq;

namespace Eklee.Azure.Functions.GraphQl
{
	public class SelectValue
	{
		public string FieldName { get; set; }

		public List<SelectValue> SelectValues { get; set; }
	}

	public class ContextValue
	{
		public List<object> Values { get; set; }

		public object GetFirstValue()
		{
			return Values.First();
		}

		public bool IsSingleValue()
		{
			return Values.Count == 1;
		}

		public bool IsMultipleValues()
		{
			return Values.Count > 1;
		}

		public Comparisons? Comparison { get; set; }

		public List<SelectValue> SelectValues { get; set; }
	}
}
