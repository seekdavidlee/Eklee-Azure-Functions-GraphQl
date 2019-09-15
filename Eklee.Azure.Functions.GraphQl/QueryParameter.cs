using Newtonsoft.Json;
using System;
using System.Collections.Generic;

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

		/// <summary>
		/// If set, this allows us to set ContextValue using this mapper func.
		/// </summary>
		[JsonIgnore]
		public Func<MapperQueryExecutionContext, List<object>> Mapper { get; set; }
	}

	/// <summary>
	/// This is derived from IGraphRequestContext.
	/// </summary>
	public class RequestContextParameter
	{
		/// <summary>
		/// Comparison.
		/// </summary>
		public Comparisons Comparison { get; set; }

		/// <summary>
		/// Value to compare with.
		/// </summary>
		public object Value { get; set; }
	}
}