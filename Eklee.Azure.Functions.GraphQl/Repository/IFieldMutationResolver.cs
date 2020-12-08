using GraphQL;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public interface IFieldMutationResolver
	{
		Task<List<TSource>> BatchAddAsync<TSource>(IResolveFieldContext<object> context, string sourceName, Func<ClaimsPrincipal, AssertAction, bool> claimsPrincipalAssertion) where TSource : class;

		Task<List<TSource>> BatchAddOrUpdateAsync<TSource>(IResolveFieldContext<object> context, string sourceName, Func<ClaimsPrincipal, AssertAction, bool> claimsPrincipalAssertion) where TSource : class;

		Task<TDeleteOutput> DeleteAsync<TSource, TDeleteInput, TDeleteOutput>(IResolveFieldContext<object> context, string sourceName,
			Func<TDeleteInput, TSource> mapDelete,
			Func<TSource, TDeleteOutput> transform,
			Func<ClaimsPrincipal, AssertAction, bool> claimsPrincipalAssertion) where TSource : class;

		Task<TSource> DeleteAsync<TSource>(IResolveFieldContext<object> context, string sourceName,
			Func<ClaimsPrincipal, AssertAction, bool> claimsPrincipalAssertion) where TSource : class;

		Task<TDeleteOutput> DeleteAllAsync<TSource, TDeleteOutput>(IResolveFieldContext<object> context, string sourceName, Func<TDeleteOutput> getOutput,
			Func<ClaimsPrincipal, AssertAction, bool> claimsPrincipalAssertion) where TSource : class;

		Task<TSource> AddAsync<TSource>(IResolveFieldContext<object> context, string sourceName,
			Func<ClaimsPrincipal, AssertAction, bool> claimsPrincipalAssertion) where TSource : class;

		Task<TSource> UpdateAsync<TSource>(IResolveFieldContext<object> context, string sourceName,
			Func<ClaimsPrincipal, AssertAction, bool> claimsPrincipalAssertion) where TSource : class;

		Task<TSource> AddOrUpdateAsync<TSource>(IResolveFieldContext<object> context, string sourceName,
			Func<ClaimsPrincipal, AssertAction, bool> claimsPrincipalAssertion) where TSource : class;
	}
}
