using System.Collections.Generic;

namespace Eklee.Azure.Functions.GraphQl
{
	public static class QueryStepExtensions
	{
		/// <summary>
		/// Specifically used the clone query steps.
		/// </summary>
		/// <param name="queryStep">queryStep.</param>
		/// <returns>QueryStep</returns>
		/// <remarks>Anytime we need to create a clone copy of query steps, this is used because deep knowldge of which property where JsonIgnoreAttribute is only known here.</remarks>
		public static QueryStep CloneQueryStep(this QueryStep queryStep)
		{
			var clone = queryStep.Clone();

			clone.ContextAction = queryStep.ContextAction;
			clone.Mapper = queryStep.Mapper;

			if (queryStep.Items != null && queryStep.Items.Count > 0)
			{
				clone.Items = new Dictionary<string, object>();
				foreach (var key in queryStep.Items.Keys)
				{
					clone.Items.Add(key, queryStep.Items[key]);
				}
			}


			// Copy Member Models which are ignored via JsonIgnore.
			for (var i = 0; i < queryStep.QueryParameters.Count; i++)
			{
				clone.QueryParameters[i].MemberModel =
					queryStep.QueryParameters[i].MemberModel;
			}

			return clone;
		}
	}
}