using System;
using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl
{
	public class ModelConventionInputBuilder<TSource> 
	{
		private readonly ObjectGraphType _objectGraphType;
		private readonly IGraphQlRepository _graphQlRepository;
		private readonly string _sourceName;
		private Action _deleteSetupAction;

		internal ModelConventionInputBuilder(
			ObjectGraphType objectGraphType,
			IGraphQlRepository graphQlRepository)
		{
			_objectGraphType = objectGraphType;
			_graphQlRepository = graphQlRepository;
			_sourceName = typeof(TSource).Name.ToLower();

			// Default setup for delete.
			_deleteSetupAction = () =>
			{
				_objectGraphType.FieldAsync<ModelConventionType<TSource>>($"delete{typeof(TSource).Name}", arguments: new QueryArguments(
						new QueryArgument<NonNullGraphType<ModelConventionInputType<TSource>>> { Name = _sourceName }
					),
					resolve: async context =>
					{
						var item = context.GetArgument<TSource>(_sourceName);
						await _graphQlRepository.DeleteAsync(item);
						return item;
					});
			};
		}

		public ModelConventionInputBuilder<TSource> Delete<TDeleteOutputType, TDeleteOutput>(Func<TSource, TDeleteOutput> transform) where TDeleteOutputType : IGraphType
		{
			_deleteSetupAction = () =>
			{
				_objectGraphType.FieldAsync<TDeleteOutputType>($"delete{typeof(TSource).Name}", arguments: new QueryArguments(
						new QueryArgument<NonNullGraphType<ModelConventionInputType<TSource>>> { Name = _sourceName }
					),
					resolve: async context =>
					{
						var item = context.GetArgument<TSource>(_sourceName);
						await _graphQlRepository.DeleteAsync(item);
						return transform(item);
					});
			};

			return this;
		}

		public ModelConventionInputBuilder<TSource> Delete<TDeleteInputType, TDeleteOutputType, TDeleteOutput>(Func<TSource, TDeleteOutput> transform) where TDeleteInputType : GraphType where TDeleteOutputType : IGraphType
		{
			_deleteSetupAction = () =>
			{
				_objectGraphType.FieldAsync<TDeleteOutputType>($"delete{typeof(TSource).Name}", arguments: new QueryArguments(
						new QueryArgument<NonNullGraphType<TDeleteInputType>> { Name = _sourceName }
					),
					resolve: async context =>
					{
						var item = context.GetArgument<TSource>(_sourceName);
						await _graphQlRepository.DeleteAsync(item);
						return transform(item);
					});
			};

			return this;
		}

		public void Build()
		{
			_objectGraphType.FieldAsync<ModelConventionType<TSource>>($"create{typeof(TSource).Name}", arguments: new QueryArguments(
					new QueryArgument<NonNullGraphType<ModelConventionInputType<TSource>>> { Name = _sourceName }
				),
				resolve: async context =>
				{
					var item = context.GetArgument<TSource>(_sourceName);
					await _graphQlRepository.AddAsync(item);
					return item;
				});

			_objectGraphType.FieldAsync<ModelConventionType<TSource>>($"update{typeof(TSource).Name}", arguments: new QueryArguments(
					new QueryArgument<NonNullGraphType<ModelConventionInputType<TSource>>> { Name = _sourceName }
				),
				resolve: async context =>
				{
					var item = context.GetArgument<TSource>(_sourceName);
					await _graphQlRepository.UpdateAsync(item);
					return item;
				});

			_deleteSetupAction();
		}
	}
}