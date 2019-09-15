using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Eklee.Azure.Functions.GraphQl
{
	public class QueryStepBuilderParameter<TProperty>
	{
		public Func<QueryExecutionContext, List<object>> Mapper;
		public Expression<Func<TProperty, object>> Expression;
	}

	public class QueryStepBuilder<TSource, TProperty>
	{
		private readonly QueryParameterBuilder<TSource> _builder;
		private readonly List<QueryStepBuilderParameter<TProperty>> _parameters =
			new List<QueryStepBuilderParameter<TProperty>>();

		private readonly Dictionary<string, object> _stepBagItems = new Dictionary<string, object>();

		public QueryStepBuilder(QueryParameterBuilder<TSource> builder)
		{
			_builder = builder;
		}

		public QueryStepBuilder<TSource, TProperty> WithProperty(Expression<Func<TProperty, object>> expression)
		{
			_parameters.Add(new QueryStepBuilderParameter<TProperty>
			{
				Expression = expression
			});
			return this;
		}

		public QueryStepBuilder<TSource, TProperty> WithPropertyFromSource(Expression<Func<TProperty, object>> expression,
			Func<QueryExecutionContext, List<object>> mapper)
		{
			_parameters.Add(new QueryStepBuilderParameter<TProperty>
			{
				Expression = expression,
				Mapper = mapper
			});
			return this;
		}

		private Action<QueryExecutionContext> _contextAction;

		/// <summary>
		/// Add interceptor for when we fire off the QueryExecutionContext so we can first transform
		/// the result before presenting to the consumer.
		/// </summary>
		/// <param name="contextAction">Context action to perform.</param>
		internal void AddQueryExecutionContextInterceptor(Action<QueryExecutionContext> contextAction)
		{
			_contextAction = contextAction;
		}

		public QueryParameterBuilder<TSource> BuildQueryResult(Action<QueryExecutionContext> contextAction)
		{
			_builder.Add(_parameters, ctx =>
			{
				_contextAction?.Invoke(ctx);
				contextAction(ctx);
			}, _stepBagItems, _skipConnectionEdgeCheck, _overrideRepositoryWithType);

			return _builder;
		}

		internal void AddStepBagItem(string key, object value)
		{
			_stepBagItems.Add(key, value);
		}

		private bool _skipConnectionEdgeCheck;
		private Type _overrideRepositoryWithType;

		/// <summary>
		/// After a search is performed, we would also perform a check on the type for
		/// connection edge settings so we can populate the appropriate fields.
		/// </summary>
		public void DisableConnectionEdgeCheck()
		{
			_skipConnectionEdgeCheck = true;
		}

		public void OverrideRepositoryTypeWith<TOverride>()
		{
			_overrideRepositoryWithType = typeof(TOverride);
		}
	}
}
