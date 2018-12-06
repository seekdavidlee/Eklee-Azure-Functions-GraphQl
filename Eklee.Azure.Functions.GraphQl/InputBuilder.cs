using System;
using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl
{
	public class InputBuilder<TInputType, TOutputType, TSource> where TInputType : GraphType where TOutputType : IGraphType
	{
		private readonly ObjectGraphType _objectGraphType;
		private readonly IGraphQlRepositoryProvider _graphQlRepository;
		private readonly string _sourceName;
		private Action _deleteSetupAction;

		internal InputBuilder(
			ObjectGraphType objectGraphType,
			IGraphQlRepositoryProvider graphQlRepository)
		{
			_objectGraphType = objectGraphType;
			_graphQlRepository = graphQlRepository;
			_sourceName = typeof(TSource).Name.ToLower();

			// Default setup for delete.
			_deleteSetupAction = () =>
			{
				_objectGraphType.FieldAsync<TOutputType>($"delete{typeof(TSource).Name}", arguments: new QueryArguments(
						new QueryArgument<NonNullGraphType<TInputType>> { Name = _sourceName }
					),
					resolve: async context =>
					{
						var item = context.GetArgument<TSource>(_sourceName);
						await _graphQlRepository.DeleteAsync(item);
						return item;
					});
			};
		}

		public InputBuilder<TInputType, TOutputType, TSource> Delete<TDeleteOutputType, TDeleteOutput>(Func<TSource, TDeleteOutput> transform) where TDeleteOutputType : IGraphType
		{
			_deleteSetupAction = () =>
			{
				_objectGraphType.FieldAsync<TDeleteOutputType>($"delete{typeof(TSource).Name}", arguments: new QueryArguments(
						new QueryArgument<NonNullGraphType<TInputType>> { Name = _sourceName }
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

		public InputBuilder<TInputType, TOutputType, TSource> Delete<TDeleteInputType, TDeleteOutputType, TDeleteOutput>(Func<TSource, TDeleteOutput> transform) where TDeleteInputType : GraphType where TDeleteOutputType : IGraphType
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
			_objectGraphType.FieldAsync<TOutputType>($"create{typeof(TSource).Name}", arguments: new QueryArguments(
					new QueryArgument<NonNullGraphType<TInputType>> { Name = _sourceName }
				),
				resolve: async context =>
				{
					var item = context.GetArgument<TSource>(_sourceName);
					await _graphQlRepository.AddAsync(item);
					return item;
				});

			_objectGraphType.FieldAsync<TOutputType>($"update{typeof(TSource).Name}", arguments: new QueryArguments(
					new QueryArgument<NonNullGraphType<TInputType>> { Name = _sourceName }
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
