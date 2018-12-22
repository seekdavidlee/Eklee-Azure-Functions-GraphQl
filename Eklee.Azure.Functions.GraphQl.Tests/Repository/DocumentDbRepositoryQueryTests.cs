using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FastMember;
using Shouldly;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests.Repository
{
	[Trait(Constants.Category, Constants.IntegrationTests)]
	public class DocumentDbRepositoryQueryTests : DocumentDbRepositoryTestsBase
	{
		public DocumentDbRepositoryQueryTests()
		{
			Expression<Func<DocumentDbFoo3, string>> expression = x => x.Category;
			var configurations = GetBaseConfigurations<DocumentDbFoo3>((MemberExpression)expression.Body);
			DocumentDbRepository.Configure(typeof(DocumentDbFoo3), configurations);
		}

		private async Task Seed()
		{
			var list = new List<DocumentDbFoo3>
			{
				new DocumentDbFoo3
				{
					Id = "1",
					Name = "Foo 1",
					Category = "cat 1",
					Description = "Ha ha ha",
					Level = 4,
					Effective = new DateTime(2017, 1, 1)
				},
				new DocumentDbFoo3
				{
					Id = "2",
					Name = "Foo 2",
					Category = "cat 1",
					Description = "Ra ha ha",
					Level = 2,
					Effective = new DateTime(2018, 1, 1)
				},
				new DocumentDbFoo3
				{
					Id = "3",
					Name = "Foo 3",
					Category = "cat 2",
					Description = "Na ha ha",
					Level = 3,
					Effective = new DateTime(2016, 1, 1),
					Expires = new DateTime(2018, 5, 1)
				},
				new DocumentDbFoo3
				{
					Id = "4",
					Name = "Foo 4",
					Category = "cat 1",
					Description = "Ba ha Ba",
					Level = 2,
					Effective = new DateTime(2014, 12, 1)
				}
			};

			foreach (var item in list)
			{
				await DocumentDbRepository.AddAsync(item);
			}
		}

		[Fact]
		public async Task CanQueryWithTwoArgs()
		{
			await Seed();

			var type = typeof(DocumentDbFoo3);
			var accessor = TypeAccessor.Create(type);
			var members = accessor.GetMembers();

			QueryParameter[] args = new[]
			{
				new QueryParameter
				{
					Comparison = Comparisons.Equals,
					ContextValue = new ContextValue { Value = "cat 1" },
					MemberModel = new ModelMember(type, accessor,
						members.Single(x=>x.Name == "Category"), false)
				},
				new QueryParameter
				{
					Comparison = Comparisons.Equals,
					ContextValue = new ContextValue { Value = 2 },
					MemberModel = new ModelMember(type, accessor,
						members.Single(x=>x.Name == "Level"), false)
				}
			};

			var results = (await DocumentDbRepository.QueryAsync<DocumentDbFoo3>("test", args)).ToList();

			results.Count.ShouldBe(2);

			var item2 = results.Single(x => x.Id == "2");

			var item4 = results.Single(x => x.Id == "4");
		}
	}
}
