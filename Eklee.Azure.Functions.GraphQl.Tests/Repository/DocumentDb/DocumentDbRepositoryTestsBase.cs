using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Eklee.Azure.Functions.GraphQl.Repository.DocumentDb;
using FastMember;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Eklee.Azure.Functions.GraphQl.Tests.Repository.DocumentDb
{
	public abstract class DocumentDbRepositoryTestsBase
	{
		protected readonly DocumentDbRepository DocumentDbRepository;

		// Will help with local testing because we will use a unique db name per session.	
		private static readonly string DatabaseId = $"db{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

		protected DocumentDbRepositoryTestsBase()
		{
			var logger = Substitute.For<ILogger>();

			DocumentDbRepository = new DocumentDbRepository(logger, new List<IDocumentDbComparison>
			{
				new DocumentDbComparisonInt(), new DocumentDbComparisonString(),
				new DocumentDbComparisonGuid(), new DocumentDbComparisonBool(),
				new DocumentDbComparisonDate()
			});
		}

		protected Dictionary<string, object> GetBaseConfigurations<TSource>(MemberExpression memberExpression)
		{
			var config = LocalConfiguration.Get().GetSection("DocumentDb");

			Console.WriteLine("DocumentDb loaded.");
			Console.WriteLine($"DocumentDb: {config["Url"]}");

			var configurations = new Dictionary<string, object>();

			configurations.Add<TSource>(DocumentDbConstants.Database, DatabaseId);
			configurations.Add<TSource>(DocumentDbConstants.Url, config["Url"]);
			configurations.Add<TSource>(DocumentDbConstants.Key, config["Key"]);
			configurations.Add<TSource>(DocumentDbConstants.RequestUnit, config["RequestUnits"]);

			configurations.Add<TSource>(DocumentDbConstants.PartitionMemberExpression, memberExpression);

			return configurations;
		}

		protected async Task<IEnumerable<T>> GetByIdAsync<T>(string id) where T : class
		{
			var type = typeof(T);
			var accessor = TypeAccessor.Create(type);
			var member = accessor.GetMembers().Single(x => x.Name == "Id");

			return await DocumentDbRepository.QueryAsync<T>("test1", new[]
			{
				new QueryParameter
				{
					ContextValue = new ContextValue { Values =new List<object>{id}, Comparison = Comparisons.Equal},
					MemberModel = new ModelMember(type, accessor, member, false)
				}
			}, null, null);
		}
	}
}