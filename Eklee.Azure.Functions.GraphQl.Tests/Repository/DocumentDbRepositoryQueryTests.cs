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
	public class DocumentDbRepositoryQueryTests : DocumentDbRepositoryTestsBase, IDisposable
	{
		private readonly Type _type = typeof(DocumentDbFoo3);
		private readonly TypeAccessor _accessor;
		private readonly MemberSet _members;

		public DocumentDbRepositoryQueryTests()
		{
			Expression<Func<DocumentDbFoo3, string>> expression = x => x.Category;
			var configurations = GetBaseConfigurations<DocumentDbFoo3>((MemberExpression)expression.Body);
			DocumentDbRepository.Configure(typeof(DocumentDbFoo3), configurations);

			_accessor = TypeAccessor.Create(_type);
			_members = _accessor.GetMembers();
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
				},
				new DocumentDbFoo3
				{
					Id = "5",
					Name = "Bar 5",
					Category = "cat 5",
					Description = "Sa Sa Ba",
					Level = 6,
					Effective = new DateTime(2014, 1, 1),
					Expires = new DateTime(2015, 12, 31)
				}
			};

			foreach (var item in list)
			{
				await DocumentDbRepository.AddAsync(item);
			}
		}

		[Fact]
		public async Task CanQueryWith_StartsWith()
		{
			await Seed();

			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue { Value = "Foo",
						Comparison = Comparisons.StringStartsWith },
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "Name"), false)
				}
			};

			var results = (await DocumentDbRepository.QueryAsync<DocumentDbFoo3>("test", args)).ToList();

			results.Count.ShouldBe(4);
			results[0].Name.ShouldStartWith("Foo");
			results[1].Name.ShouldStartWith("Foo");
			results[2].Name.ShouldStartWith("Foo");
			results[3].Name.ShouldStartWith("Foo");
		}

		[Fact]
		public async Task CanQueryWith_EndsWith()
		{
			await Seed();

			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue { Value = "ha ha",
						Comparison = Comparisons.StringEndsWith},
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "Description"), false)
				}
			};

			var results = (await DocumentDbRepository.QueryAsync<DocumentDbFoo3>("test", args)).ToList();

			results.Count.ShouldBe(3);
			results[0].Description.ShouldEndWith("ha ha");
			results[1].Description.ShouldEndWith("ha ha");
			results[2].Description.ShouldEndWith("ha ha");
		}

		[Fact]
		public async Task CanQueryWith_Contains()
		{
			await Seed();

			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue { Value = "ha",
						Comparison = Comparisons.StringContains },
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "Description"), false)
				}
			};

			var results = (await DocumentDbRepository.QueryAsync<DocumentDbFoo3>("test", args)).ToList();

			results.Count.ShouldBe(4);
			results[0].Description.ShouldContain("ha");
			results[1].Description.ShouldContain("ha");
			results[2].Description.ShouldContain("ha");
			results[3].Description.ShouldContain("ha");
		}

		[Fact]
		public async Task CanQueryWith_TwoArgsOfTypesStringAndInt()
		{
			await Seed();

			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue
					{
						Value = "cat 1",
						Comparison = Comparisons.Equal
					},
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "Category"), false)
				},
				new QueryParameter
				{
					ContextValue = new ContextValue {
						Value = 2,
						Comparison = Comparisons.Equal },
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "Level"), false)
				}
			};

			var results = (await DocumentDbRepository.QueryAsync<DocumentDbFoo3>("test", args)).ToList();

			results.Count.ShouldBe(2);

			var item2 = results.Single(x => x.Id == "2");
			item2.Category.ShouldBe("cat 1");
			item2.Level.ShouldBe(2);

			var item4 = results.Single(x => x.Id == "4");
			item4.Category.ShouldBe("cat 1");
			item4.Level.ShouldBe(2);
		}

		public void Dispose()
		{
			DocumentDbRepository.DeleteAllAsync<DocumentDbFoo3>()
				.GetAwaiter().GetResult();
		}
	}
}
