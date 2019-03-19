using System;
using Microsoft.Azure.Documents;
using System.Linq;

namespace Eklee.Azure.Functions.GraphQl.Repository.DocumentDb
{
	public abstract class BaseDocumentDbComparison<T> : IDocumentDbComparison
	{
		protected T Value;
		protected T[] Values;

		protected QueryParameter QueryParameter;

		private string[] GetVariableNames()
		{
			if (Values != null && Values.Length > 1)
			{
				return Values.Select(x => $"@{NextCounter()}").ToArray();
			}

			return new string[] { $"@{NextCounter()}" };
		}

		private string NextCounter()
		{
			return $"p{Guid.NewGuid().ToString("N")}";
		}

		protected virtual bool AssertContextValue(T value)
		{
			return true;
		}

		protected string GetPropertyName()
		{
			return $"x.{QueryParameter.MemberModel.Member.Name}";
		}

		public bool CanHandle(QueryParameter queryParameter)
		{
			// Reset from previous session.
			Value = default(T);
			Values = null;

			QueryParameter = queryParameter;

			if (QueryParameter.ContextValue.IsSingleValue() &&
				QueryParameter.ContextValue.GetFirstValue() is T value &&
				AssertContextValue(value))
			{
				Value = value;
				return true;
			}

			if (QueryParameter.ContextValue.IsMultipleValues() &&
				QueryParameter.ContextValue.GetFirstValue() is T)
			{
				Values = QueryParameter.ContextValue.Values.Select(x => (T)x).ToArray();
				return true;
			}

			return false;
		}

		public DocumentDbSqlParameter Generate()
		{
			if (!QueryParameter.ContextValue.Comparison.HasValue)
			{
				return null;
			}

			var names = GetVariableNames();

			var comparison = GetComprisonString(QueryParameter.ContextValue.Comparison.Value, names);

			if (comparison == null) return null;

			return new DocumentDbSqlParameter
			{
				Comparison = comparison,
				SqlParameters = names.Length == 1 ?
					new SqlParameter[] { new SqlParameter(names[0], Value) } :
					names.Select((name, index) => new SqlParameter { Name = name, Value = Values[index] }).ToArray()
			};
		}

		protected abstract string GetComprisonString(Comparisons comparison, string[] names);
	}
}
