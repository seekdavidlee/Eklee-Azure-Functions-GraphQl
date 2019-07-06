using System;
using System.Security.Claims;
using Eklee.Azure.Functions.GraphQl.Repository;
using Eklee.Azure.Functions.GraphQl.Repository.DocumentDb;
using Eklee.Azure.Functions.GraphQl.Repository.Http;
using Eklee.Azure.Functions.GraphQl.Repository.InMemory;
using Eklee.Azure.Functions.GraphQl.Repository.Search;
using Eklee.Azure.Functions.GraphQl.Repository.TableStorage;
using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl
{
	public class ModelConventionInputBuilder<TSource> : IModelConventionInputBuilder<TSource> where TSource : class
	{
		private readonly ObjectGraphType _objectGraphType;
		private readonly IGraphQlRepositoryProvider _graphQlRepositoryProvider;
		private readonly string _sourceName;
		private Action _deleteSetupAction;
		private readonly ISearchMappedModels _searchMappedModels;
		private readonly IQueryArgumentsBuilder _queryArgumentsBuilder;
		private readonly IFieldMutationResolver _fieldMutationResolver; // Singleton, so don't store anything which requires a per instance behavior.
		private Func<ClaimsPrincipal, AssertAction, bool> _claimsPrincipalAssertion;

		internal ModelConventionInputBuilder(
			ObjectGraphType objectGraphType,
			IGraphQlRepositoryProvider graphQlRepositoryProviderProvider,
			ISearchMappedModels searchMappedModels,
			IQueryArgumentsBuilder queryArgumentsBuilder,
			IFieldMutationResolver fieldMutationResolver)
		{
			_objectGraphType = objectGraphType;
			_graphQlRepositoryProvider = graphQlRepositoryProviderProvider;
			_searchMappedModels = searchMappedModels;
			_queryArgumentsBuilder = queryArgumentsBuilder;
			_fieldMutationResolver = fieldMutationResolver;
			_sourceName = typeof(TSource).Name.ToLower();

			// Default setup for delete.
			_deleteSetupAction = () =>
			{
				var fieldName = $"delete{GetTypeName()}";

				if (_objectGraphType.HasField(fieldName)) return;

				_objectGraphType.FieldAsync<ModelConventionType<TSource>>(fieldName,
					description: $"Deletes a single {GetTypeName()} instance.",
					arguments: _queryArgumentsBuilder.BuildNonNull<TSource>(_sourceName),
					resolve: async context => await _fieldMutationResolver.DeleteAsync<TSource>(context, _sourceName, _claimsPrincipalAssertion));
			};
		}

		public ModelConventionInputBuilder<TSource> AssertWithClaimsPrincipal(Func<ClaimsPrincipal, AssertAction, bool> claimsPrincipalAssertion)
		{
			_claimsPrincipalAssertion = claimsPrincipalAssertion;
			return this;
		}

		private IGraphQlRepository _graphQlRepository;
		private Type _typeSource;

		public InMemoryConfiguration<TSource> ConfigureInMemory<TType>()
		{
			_graphQlRepository = _graphQlRepositoryProvider.Use<TType, InMemoryRepository>();
			_typeSource = typeof(TType);
			return new InMemoryConfiguration<TSource>(this);
		}

		public TableStorageConfiguration<TSource> ConfigureTableStorage<TType>()
		{
			_graphQlRepository = _graphQlRepositoryProvider.Use<TType, TableStorageRepository>();
			_typeSource = typeof(TType);
			return new TableStorageConfiguration<TSource>(this, _graphQlRepository, _typeSource);
		}

		public HttpConfiguration<TSource> ConfigureHttp<TType>()
		{
			_graphQlRepository = _graphQlRepositoryProvider.Use<TType, HttpRepository>();
			_typeSource = typeof(TType);
			return new HttpConfiguration<TSource>(this, _graphQlRepository, _typeSource);
		}

		public DocumentDbConfiguration<TSource> ConfigureDocumentDb<TType>()
		{
			_graphQlRepository = _graphQlRepositoryProvider.Use<TType, DocumentDbRepository>();
			_typeSource = typeof(TType);
			return new DocumentDbConfiguration<TSource>(this, _graphQlRepository, _typeSource);
		}

		public SearchConfiguration<TSource> ConfigureSearch<TSearchModel>()
		{
			_graphQlRepositoryProvider.Use<SearchModel, SearchRepository>();

			_graphQlRepository = _graphQlRepositoryProvider.Use<TSearchModel, SearchRepository>();
			_typeSource = typeof(TSearchModel);
			return new SearchConfiguration<TSource>(this, _graphQlRepository);
		}

		public SearchConfiguration<TSource> ConfigureSearchWith<TSearchModel, TModel>()
		{
			_searchMappedModels.Map<TSearchModel, TModel>();
			return ConfigureSearch<TSearchModel>();
		}

		public ModelConventionInputBuilder<TSource> Delete<TDeleteInput, TDeleteOutput>(
			Func<TDeleteInput, TSource> mapDelete,
			Func<TSource, TDeleteOutput> transform)
		{
			_deleteSetupAction = () =>
			{
				var fieldName = $"delete{GetTypeName()}";

				if (_objectGraphType.HasField(fieldName)) return;

				_objectGraphType.FieldAsync<ModelConventionType<TDeleteOutput>>(fieldName,
					description: $"Deletes a single {GetTypeName()} instance.",
					arguments: _queryArgumentsBuilder.BuildNonNull<TDeleteInput>(_sourceName),
					resolve: async context => await _fieldMutationResolver.DeleteAsync(
						context, _sourceName, mapDelete, transform, _claimsPrincipalAssertion));
			};

			return this;
		}

		private Action _deleteAllAction;

		public ModelConventionInputBuilder<TSource> DeleteAll<TDeleteOutput>(Func<TDeleteOutput> getOutput)
		{
			_deleteAllAction = () =>
			{
				var fieldName = $"deleteAll{GetTypeName()}";

				if (_objectGraphType.HasField(fieldName)) return;

				_objectGraphType.FieldAsync<ModelConventionType<TDeleteOutput>>(fieldName,
					description: $"Deletes all {GetTypeName()} instances.",
					resolve: async context => await _fieldMutationResolver.DeleteAllAsync<TSource, TDeleteOutput>(
						context, _sourceName, getOutput, _claimsPrincipalAssertion));
			};
			return this;
		}

		private string GetTypeName()
		{
			return typeof(TSource).Name;
		}

		private void AddBatchCreateField()
		{
			var fieldName = $"batchCreate{GetTypeName()}";

			if (_objectGraphType.HasField(fieldName)) return;

			_objectGraphType.FieldAsync<ListGraphType<ModelConventionType<TSource>>>(fieldName,
				description: $"Batch create {GetTypeName()} instances.",
				arguments: _queryArgumentsBuilder.BuildList<TSource>(_sourceName),
				resolve: async context => await _fieldMutationResolver.BatchAddAsync<TSource>(
					context, _sourceName, _claimsPrincipalAssertion));
		}

		private void AddBatchCreateOrUpdateField()
		{
			var fieldName = $"batchCreateOrUpdate{GetTypeName()}";

			if (_objectGraphType.HasField(fieldName)) return;

			_objectGraphType.FieldAsync<ListGraphType<ModelConventionType<TSource>>>(fieldName,
				description: $"Batch create or update {GetTypeName()} instances.",
				arguments: _queryArgumentsBuilder.BuildList<TSource>(_sourceName),
				resolve: async context => await _fieldMutationResolver.BatchAddOrUpdateAsync<TSource>(
					context, _sourceName, _claimsPrincipalAssertion));
		}

		private void AddCreateField()
		{
			var fieldName = $"create{typeof(TSource).Name}";

			if (_objectGraphType.HasField(fieldName)) return;

			_objectGraphType.FieldAsync<ModelConventionType<TSource>>(fieldName,
				description: $"Creates a single {GetTypeName()} instance.",
				arguments: _queryArgumentsBuilder.BuildNonNull<TSource>(_sourceName),
				resolve: async context => await _fieldMutationResolver.AddAsync<TSource>(
					context, _sourceName, _claimsPrincipalAssertion));
		}

		private void AddCreateOrUpdateField()
		{
			var fieldName = $"createOrUpdate{typeof(TSource).Name}";

			if (_objectGraphType.HasField(fieldName)) return;

			_objectGraphType.FieldAsync<ModelConventionType<TSource>>(fieldName,
				description: $"Creates or updates a single {GetTypeName()} instance.",
				arguments: _queryArgumentsBuilder.BuildNonNull<TSource>(_sourceName),
				resolve: async context => await _fieldMutationResolver.AddOrUpdateAsync<TSource>(
					context, _sourceName, _claimsPrincipalAssertion));
		}

		private void AddUpdateField()
		{
			var fieldName = $"update{GetTypeName()}";

			if (_objectGraphType.HasField(fieldName)) return;

			_objectGraphType.FieldAsync<ModelConventionType<TSource>>(fieldName,
				description: $"Updates a single {GetTypeName()} instance.",
				arguments: _queryArgumentsBuilder.BuildNonNull<TSource>(_sourceName),
				resolve: async context => await _fieldMutationResolver.UpdateAsync<TSource>(
					context, _sourceName, _claimsPrincipalAssertion));
		}

		private bool _disableBatchCreate;

		public ModelConventionInputBuilder<TSource> DisableBatchCreate()
		{
			_disableBatchCreate = true;
			return this;
		}

		private bool _disableCreate;

		public ModelConventionInputBuilder<TSource> DisableCreate()
		{
			_disableCreate = true;
			return this;
		}

		private bool _disableUpdate;

		public ModelConventionInputBuilder<TSource> DisableUpdate()
		{
			_disableUpdate = true;
			return this;
		}

		private bool _disableCreateOrUpdate;

		public ModelConventionInputBuilder<TSource> DisableCreateOrUpdate()
		{
			_disableCreateOrUpdate = true;
			return this;
		}

		private bool _disableDelete;

		public ModelConventionInputBuilder<TSource> DisableDelete()
		{
			_disableDelete = true;
			return this;
		}

		private bool _disableBatchCreateOrUpdate;

		public ModelConventionInputBuilder<TSource> DisableBatchCreateOrUpdate()
		{
			_disableBatchCreateOrUpdate = true;
			return this;
		}

		public void Build()
		{
			if (!_disableBatchCreate) AddBatchCreateField();

			if (!_disableCreate) AddCreateField();

			if (!_disableUpdate) AddUpdateField();

			if (!_disableCreateOrUpdate) AddCreateOrUpdateField();

			if (!_disableDelete) _deleteSetupAction();

			if (!_disableBatchCreateOrUpdate) AddBatchCreateOrUpdateField();

			_deleteAllAction?.Invoke();
		}
	}
}