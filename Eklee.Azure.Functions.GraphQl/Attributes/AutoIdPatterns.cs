namespace Eklee.Azure.Functions.GraphQl.Attributes
{
	/// <summary>
	/// Use one of the AutoIdPatterns available.
	/// </summary>
	public enum AutoIdPatterns
	{
		/// <summary>
		/// Guid Id.
		/// </summary>
		Guid,

		/// <summary>
		/// Custom implementation that you define.
		/// </summary>
		Custom
	}
}
