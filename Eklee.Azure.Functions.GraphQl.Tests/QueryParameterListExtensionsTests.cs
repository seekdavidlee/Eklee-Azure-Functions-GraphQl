using System.Collections.Generic;
using Shouldly;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests
{
	public class QueryFoo
	{
		public string Id { get; set; }
	}

	public class QueryParameterListExtensionsTests
	{
		[Fact]
		public void CacheKeyShouldBeGeneratedUsingValuesFromList()
		{
			var steps = new List<QueryStep>
			{
				new QueryStep
				{
					QueryParameters = new List<QueryParameter>
					{
						new QueryParameter
						{
							ContextValue = new ContextValue
							{
								Comparison = Comparisons.Equal,
								Values = new List<object>{ "ABB90","CCE5"}
							}
						}
					}
				}
			};

			var cacheKey = steps.GetCacheKey<QueryFoo>();
			cacheKey.ShouldContain("ABB90");
			cacheKey.ShouldContain("CCE5");
		}
	}
}
