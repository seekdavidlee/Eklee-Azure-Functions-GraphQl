using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public interface IFieldMutationResolver
	{
		Task<List<TSource>> BatchAddAsync<TSource>(ResolveFieldContext<object> context, string sourceName) where TSource : class;

		Task<List<TSource>> BatchAddOrUpdateAsync<TSource>(ResolveFieldContext<object> context, string sourceName) where TSource : class;

		Task<TDeleteOutput> DeleteAsync<TSource, TDeleteInput, TDeleteOutput>(ResolveFieldContext<object> context, string sourceName,
			Func<TDeleteInput, TSource> mapDelete,
			Func<TSource, TDeleteOutput> transform) where TSource : class;

		Task<TSource> DeleteAsync<TSource>(ResolveFieldContext<object> context, string sourceName) where TSource : class;

		Task<TDeleteOutput> DeleteAllAsync<TSource, TDeleteOutput>(ResolveFieldContext<object> context, string sourceName, Func<TDeleteOutput> getOutput) where TSource : class;

		Task<TSource> AddAsync<TSource>(ResolveFieldContext<object> context, string sourceName) where TSource : class;

		Task<TSource> UpdateAsync<TSource>(ResolveFieldContext<object> context, string sourceName) where TSource : class;

		Task<TSource> AddOrUpdateAsync<TSource>(ResolveFieldContext<object> context, string sourceName) where TSource : class;

		Func<ClaimsPrincipal, AssertAction, bool> ClaimsPrincipalAssertion { get; set; }
	}
}
