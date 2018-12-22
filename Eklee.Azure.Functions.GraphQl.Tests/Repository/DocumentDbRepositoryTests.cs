using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Eklee.Azure.Functions.GraphQl.Repository;
using FastMember;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests.Repository
{
	[Trait(Constants.Category, Constants.IntegrationTests)]
	public class DocumentDbRepositoryTests
	{
		private readonly DocumentDbRepository _documentDbRepository;

		// Will help with local testing because we will use a unique db name per session.	
		private static readonly string DatabaseId = $"db{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

		public DocumentDbRepositoryTests()
		{
			var logger = Substitute.For<ILogger>();

			_documentDbRepository = new DocumentDbRepository(logger);
		}

		private Dictionary<string, object> GetBaseConfigurations<TSource>(MemberExpression memberExpression)
		{
			var configurations = new Dictionary<string, object>();

			configurations.Add<TSource>(DocumentDbConstants.Database, DatabaseId);
			configurations.Add<TSource>(DocumentDbConstants.Url, "https://localhost:8081");
			configurations.Add<TSource>(DocumentDbConstants.Key, "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");
			configurations.Add<TSource>(DocumentDbConstants.RequestUnit, "400");

			configurations.Add<TSource>(DocumentDbConstants.PartitionMemberExpression, memberExpression);

			return configurations;
		}

		[Fact]
		public async Task CanHandleDocumentTypeWithStringBasedPartition()
		{
			Expression<Func<DocumentDbFoo1, string>> expression = x => x.MyStringCategory;
			var configurations = GetBaseConfigurations<DocumentDbFoo1>((MemberExpression)expression.Body);

			_documentDbRepository.Configure(typeof(DocumentDbFoo1), configurations);

			const string id = "3";

			await _documentDbRepository.AddAsync(new DocumentDbFoo1
			{
				Id = id,
				Name = "Foo 1",
				MyStringCategory = "cat 1"
			});

			await _documentDbRepository.UpdateAsync(new DocumentDbFoo1
			{
				Id = id,
				Name = "Foo 1 v2",
				MyStringCategory = "cat 1"
			});

			var item = (await GetByIdAsync<DocumentDbFoo1>(id)).Single();

			item.Name.ShouldBe("Foo 1 v2");

			_documentDbRepository.DeleteAllAsync<DocumentDbFoo1>().GetAwaiter().GetResult();
		}

		[Fact]
		public async Task CanHandleDocumentTypeWithIntBasedPartition()
		{
			Expression<Func<DocumentDbFoo2, int>> expression = x => x.MyIntCategory;
			var configurations = GetBaseConfigurations<DocumentDbFoo2>((MemberExpression)expression.Body);

			_documentDbRepository.Configure(typeof(DocumentDbFoo2), configurations);

			const string id = "2";

			await _documentDbRepository.AddAsync(new DocumentDbFoo2
			{
				Id = id,
				Name = "Foo 2",
				MyIntCategory = 2
			});

			await _documentDbRepository.UpdateAsync(new DocumentDbFoo2
			{
				Id = id,
				Name = "Foo 2 v2",
				MyIntCategory = 2
			});

			var item = (await GetByIdAsync<DocumentDbFoo2>(id)).Single();

			item.Name.ShouldBe("Foo 2 v2");

			_documentDbRepository.DeleteAllAsync<DocumentDbFoo2>().GetAwaiter().GetResult();
		}

		private async Task<IEnumerable<T>> GetByIdAsync<T>(string id)
		{
			var type = typeof(T);
			var accessor = TypeAccessor.Create(type);
			var member = accessor.GetMembers().Single(x => x.Name == "Id");

			return await _documentDbRepository.QueryAsync<T>("test1", new[]
			{
				new QueryParameter
				{
					Comparison = Comparisons.Equals,
					ContextValue = new ContextValue { Value = id },
					MemberModel = new ModelMember(type, accessor, member,false )
				}
			});
		}
	}
}
