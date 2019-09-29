using Eklee.Azure.Functions.GraphQl.Repository.Search;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Eklee.Azure.Functions.GraphQl.Queries
{
	public class ContextValueResolver : IContextValueResolver
	{
		public ContextValue GetContextValue(ResolveFieldContext<object> context, ModelMember modelMember, ContextValueSetRule rule)
		{
			var name = modelMember.Name;

			var contextValue = new ContextValue();

			if (rule == null || !rule.DisableSetSelectValues)
				contextValue.PopulateSelectValues(context);

			if (context.Arguments == null) return contextValue;

			var args = context.Arguments;
			if (args.ContainsKey(name))
			{
				if (args[name] is Dictionary<string, object> arg)
				{
					if (modelMember.IsGuid)
					{
						contextValue.Values = new List<object> { Guid.Parse(arg.First().Value.ToString()) };
					}
					else
					{
						contextValue.Values = new List<object> { arg.First().Value };
					}

					if (contextValue.Values == null)
					{
						throw new ArgumentNullException($"{name}.Value");
					}

					string comparison = arg.First().Key;
					if (comparison == Constants.Equal)
					{
						contextValue.Comparison = Comparisons.Equal;
						return contextValue;
					}

					if (comparison == Constants.Contains && contextValue.GetFirstValue() is string)
					{
						contextValue.Comparison = Comparisons.StringContains;
						return contextValue;
					}

					if (comparison == Constants.StartsWith && contextValue.GetFirstValue() is string)
					{
						contextValue.Comparison = Comparisons.StringStartsWith;
						return contextValue;
					}

					if (comparison == Constants.EndsWith && contextValue.GetFirstValue() is string)
					{
						contextValue.Comparison = Comparisons.StringEndsWith;
						return contextValue;
					}

					if (comparison == Constants.NotEqual && (
							contextValue.GetFirstValue() is int ||
							contextValue.GetFirstValue() is DateTime))
					{
						contextValue.Comparison = Comparisons.NotEqual;
						return contextValue;
					}

					if (comparison == Constants.GreaterThan && (
							contextValue.GetFirstValue() is int ||
							contextValue.GetFirstValue() is DateTime))
					{
						contextValue.Comparison = Comparisons.GreaterThan;
						return contextValue;
					}

					if (comparison == Constants.GreaterEqualThan && (
							contextValue.GetFirstValue() is int ||
							contextValue.GetFirstValue() is DateTime))
					{
						contextValue.Comparison = Comparisons.GreaterEqualThan;
						return contextValue;
					}

					if (comparison == Constants.LessThan && (
							contextValue.GetFirstValue() is int ||
							contextValue.GetFirstValue() is DateTime))
					{
						contextValue.Comparison = Comparisons.LessThan;
						return contextValue;
					}

					if (comparison == Constants.LessEqualThan && (
							contextValue.GetFirstValue() is int ||
							contextValue.GetFirstValue() is DateTime))
					{
						contextValue.Comparison = Comparisons.LessEqualThan;
						return contextValue;
					}
					throw new NotImplementedException($"Comparison: {comparison} is not implemented for type {contextValue.GetFirstValue().GetType().Name}.");
				}

				if (args[name] is IEnumerable<object> listArg)
				{
					var searchFilterModels = new List<object>();
					foreach (Dictionary<string, object> item in listArg)
					{
						if (item.ContainsKey(Constants.FieldName) &&
							item.ContainsKey(Constants.FieldValue))
						{
							searchFilterModels.Add(new SearchFilterModel
							{
								FieldName = (string)item[Constants.FieldName],
								Value = (string)item[Constants.FieldValue],
								Comprison = (Comparisons)(int)item[Constants.Comparison]
							});
						}
					}

					contextValue.Values = searchFilterModels;
					return contextValue;
				}

				throw new NotImplementedException($"{name} type not supported.");
			}

			return contextValue;
		}
	}
}
