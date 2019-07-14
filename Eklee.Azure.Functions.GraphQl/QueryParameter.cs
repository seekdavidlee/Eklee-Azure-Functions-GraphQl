using Newtonsoft.Json;

namespace Eklee.Azure.Functions.GraphQl
{
	/// <summary>
	/// The QueryParameter class is used to define the parameter to query against any type of supported repository.
	/// </summary>
	public class QueryParameter
	{
		/// <summary>
		/// MemberModel keeps information about the particular property we are dealing with as we are querying.
		/// </summary>
		[JsonIgnore]
		public ModelMember MemberModel { get; set; }

		/// <summary>
		/// This is where we set the query value coming from GraphQL input.
		/// </summary>
		public ContextValue ContextValue { get; set; }

		/// <summary>
		/// This property is used to provide support for behavioral settings for how we configure ContextValue.
		/// </summary>
		public ContextValueSetRule Rule { get; set; }
	}
}