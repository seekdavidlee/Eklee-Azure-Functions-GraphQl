using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Eklee.Azure.Functions.GraphQl.Repository.DocumentDb;

namespace Eklee.Azure.Functions.GraphQl.Repository.TableStorage
{
	public class TableStorageConfiguration<TSource> where TSource : class
	{
		private readonly IModelConventionInputBuilder<TSource> _modelConventionInputBuilder;
		private readonly IGraphQlRepository _graphQlRepository;
		private readonly Type _typeSource;
		private readonly Dictionary<string, object> _configurations = new Dictionary<string, object>();

		public TableStorageConfiguration(IModelConventionInputBuilder<TSource> modelConventionInputBuilder,
			IGraphQlRepository graphQlRepository,
			Type typeSource)
		{
			_modelConventionInputBuilder = modelConventionInputBuilder;
			_graphQlRepository = graphQlRepository;
			_typeSource = typeSource;
		}

		public TableStorageConfiguration<TSource> AddPrefix(string prefix)
		{
			_configurations.Add<TSource>(TableStorageConstants.Prefix, prefix);
			return this;
		}

		public TableStorageConfiguration<TSource> AddConnectionString(string connectionString)
		{
			_configurations.Add<TSource>(TableStorageConstants.ConnectionString, connectionString);
			return this;
		}

		public TableStorageConfiguration<TSource> AddPartition<TProperty>(Expression<Func<TSource, TProperty>> expression)
		{
			if (expression.Body is MemberExpression memberExpression)
			{
				_configurations.Add<TSource>(TableStorageConstants.PartitionMemberExpression, memberExpression);
			}

			return this;
		}

		public TableStorageConfiguration<TSource> AddGraphRequestContextSelector(Func<IGraphRequestContext, bool> selector)
		{
			_configurations.Add<TSource>(TableStorageConstants.RequestContextSelector, selector);
			return this;
		}

		public IModelConventionInputBuilder<TSource> BuildTableStorage()
		{
			if (!_configurations.ContainsKey<TSource>(TableStorageConstants.PartitionMemberExpression))
			{
				throw new InvalidOperationException("Partition is not set!");
			}

			_graphQlRepository.Configure(_typeSource, _configurations);

			return _modelConventionInputBuilder;
		}
	}
}
