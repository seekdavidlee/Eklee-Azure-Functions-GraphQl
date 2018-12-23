namespace Eklee.Azure.Functions.GraphQl
{
	public class ContextValue
	{
		public bool IsNotSet { get; set; }
		public object Value { get; set; }
		public Comparisons? Comparison { get; set; }
	}

	public enum Comparisons
	{
		Equal,
		StringStartsWith,
		StringEndsWith,
		StringContains
	}
}
