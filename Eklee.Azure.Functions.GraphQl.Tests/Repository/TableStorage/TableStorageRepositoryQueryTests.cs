using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FastMember;
using Shouldly;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests.Repository.TableStorage
{
	[Collection(Constants.TableStorageTests)]
	[Trait(Constants.Category, Constants.IntegrationTests)]
	public class TableStorageRepositoryQueryTests : TableStorageRepositoryTestsBase, IDisposable
	{
		private readonly Type _type = typeof(DocumentDbFoo3);
		private readonly TypeAccessor _accessor;
		private readonly MemberSet _members;

		public TableStorageRepositoryQueryTests()
		{
			Expression<Func<DocumentDbFoo3, string>> expression = x => x.Category;

			"TableStorageRepositoryQueryTests instantiated and loading config.".Log();
			var configurations = GetBaseConfigurations<DocumentDbFoo3>((MemberExpression)expression.Body);

			"Configuring TableStorageRepository.".Log();
			TableStorageRepository.Configure(typeof(DocumentDbFoo3), configurations);

			"Creating TableStorageRepositoryQueryTests accessor type.".Log();

			_accessor = TypeAccessor.Create(_type);
			_members = _accessor.GetMembers();

			"TableStorageRepositoryQueryTests constructor.".Log();
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
					Effective = new DateTime(2017, 1, 1).ToUtc(),
					TypeId = Guid.Parse("AF359AD6-9B8E-43F7-B898-35CEB400051A")
				},
				new DocumentDbFoo3
				{
					Id = "2",
					Name = "Foo 2",
					Category = "cat 1",
					Description = "Ra ha ha",
					Level = 2,
					Effective = new DateTime(2018, 1, 1).ToUtc(),
					TypeId = Guid.Parse("9B522321-B689-43EC-A3DC-DC17EE2A42DD")
				},
				new DocumentDbFoo3
				{
					Id = "3",
					Name = "Foo 3",
					Category = "cat 2",
					Description = "Na ha ha",
					Level = 3,
					Effective = new DateTime(2016, 1, 1).ToUtc(),
					Expires = new DateTime(2018, 5, 1).ToUtc(),
					TypeId = Guid.Parse("CC562BC9-C2E5-46D8-B701-E5D57F5800B8")
				},
				new DocumentDbFoo3
				{
					Id = "4",
					Name = "Foo 4",
					Category = "cat 1",
					Description = "Ba ha Ba",
					Level = 2,
					Effective = new DateTime(2014, 12, 1).ToUtc(),
					TypeId = Guid.Parse("4AB97B79-37DB-4AB7-BC32-D3EB1780C8AF")
				},
				new DocumentDbFoo3
				{
					Id = "5",
					Name = "Bar 5",
					Category = "cat 5",
					Description = "Sa Sa Ba",
					Level = 6,
					Effective = new DateTime(2014, 1, 1).ToUtc(),
					Expires = new DateTime(2015, 12, 31).ToUtc(),
					TypeId = Guid.Parse("9B522321-B689-43EC-A3DC-DC17EE2A42DD")
				},
				new DocumentDbFoo3
				{
					Id = "6",
					Name = "Nest 6",
					Category = "ele 6",
					Description = "JUST ALP",
					Level = 8,
					Effective = new DateTime(2019, 1, 1).ToUtc(),
					Expires = new DateTime(2020, 12, 31).ToUtc(),
					TypeId = Guid.Parse("DC742320-F3A4-4DE1-9239-6A9EC68BD430"),
					IsActive = true
				},
				new DocumentDbFoo3
				{
					Id = "7",
					Name = "Desk 7",
					Category = "Mal 7",
					Description = "KO PO LA",
					Level = 9,
					Effective = new DateTime(2019, 4, 1).ToUtc(),
					Expires = new DateTime(2020, 11, 5).ToUtc(),
					TypeId = Guid.Parse("D4FB1574-05CF-4422-9477-13D722A5448E"),
					IsActive = true
				},
				new DocumentDbFoo3
				{
					Id = "8",
					Name = "Desk 8",
					Category = "Pal 8",
					Description = "IN AS JA",
					Level = 9,
					Effective = new DateTime(2019, 4, 15).ToUtc(),
					Expires = new DateTime(2021, 11, 9).ToUtc(),
					TypeId = Guid.Parse("DE6EFD89-2D62-4E63-98D2-AEA9E622B223"),
					IsActive = true
				}
			};

			"Seeding Table Storage.".Log();

			foreach (var item in list)
			{
				await TableStorageRepository.AddAsync(item, null);
			}

			"Table Storage has been seeded.".Log();
		}

		[Fact]
		public async Task CanQueryWithMultipleStringEquals()
		{
			await Seed();

			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue { Values = new List<object>{ "Bar 5"},
						Comparison = Comparisons.Equal },
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "Name"), false)
				},
				new QueryParameter
				{
					ContextValue = new ContextValue { Values = new List<object>{ "cat 5"},
						Comparison = Comparisons.Equal },
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "Category"), false)
				},
				new QueryParameter
				{
					ContextValue = new ContextValue { Values =new List<object>{  "Sa Sa Ba"},
						Comparison = Comparisons.Equal },
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "Description"), false)
				}
			};

			var results = (await TableStorageRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(1);
			results[0].Id.ShouldBe("5");
		}

		[Fact]
		public async Task CanQueryWithSingleStringEquals()
		{
			await Seed();

			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue { Values = new List<object>{ "Bar 5"},
						Comparison = Comparisons.Equal },
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "Name"), false)
				}
			};

			var results = (await TableStorageRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(1);
			results[0].Id.ShouldBe("5");
		}

		[Fact]
		public async Task CanQueryWithSingleIntEquals()
		{
			await Seed();

			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue { Values = new List<object>{ 6},
						Comparison = Comparisons.Equal },
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "Level"), false)
				}
			};

			var results = (await TableStorageRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(1);
			results[0].Id.ShouldBe("5");
		}

		[Fact]
		public async Task CanQueryWithSingleIntGreaterThanUsingMax()
		{
			await Seed();

			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue { Values =new List<object>{  9},
						Comparison = Comparisons.GreaterThan },
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "Level"), false)
				}
			};

			var results = (await TableStorageRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(0);
		}

		[Fact]
		public async Task CanQueryWithSingleIntGreaterThan()
		{
			await Seed();

			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue { Values =new List<object>{ 8 },
						Comparison = Comparisons.GreaterThan },
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "Level"), false)
				}
			};

			var results = (await TableStorageRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(2);
			results.Any(x => x.Id == "7").ShouldBeTrue();
			results.Any(x => x.Id == "8").ShouldBeTrue();
		}

		[Fact]
		public async Task CanQueryWithSingleIntGreaterEqualThan()
		{
			await Seed();

			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue { Values = new List<object>{ 8 },
						Comparison = Comparisons.GreaterEqualThan },
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "Level"), false)
				}
			};

			var results = (await TableStorageRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(3);
			results.Any(x => x.Id == "6").ShouldBeTrue();
			results.Any(x => x.Id == "7").ShouldBeTrue();
			results.Any(x => x.Id == "8").ShouldBeTrue();
		}

		[Fact]
		public async Task CanQueryWithSingleIntLessEqualThan()
		{
			await Seed();

			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue { Values =new List<object>{  2},
						Comparison = Comparisons.LessEqualThan },
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "Level"), false)
				}
			};

			var results = (await TableStorageRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(2);
			results.Any(x => x.Id == "4").ShouldBe(true);
			results.Any(x => x.Id == "2").ShouldBe(true);
		}

		[Fact]
		public async Task CanQueryWithSingleIntLessThan()
		{
			await Seed();

			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue { Values =new List<object>{  2},
						Comparison = Comparisons.LessThan },
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "Level"), false)
				}
			};

			var results = (await TableStorageRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(0);
		}

		[Fact]
		public async Task CanQueryWithSingleIntLessEqualThanWithStringEqual()
		{
			await Seed();

			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue { Values =new List<object>{  2},
						Comparison = Comparisons.LessEqualThan },
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "Level"), false)
				},
				new QueryParameter
				{
					ContextValue = new ContextValue { Values =new List<object>{  "Foo 4"},
						Comparison = Comparisons.Equal },
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "Name"), false)
				}
			};

			var results = (await TableStorageRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(1);
			results[0].Id.ShouldBe("4");
		}

		[Fact]
		public async Task CanQueryWithSingleDateEquals()
		{
			await Seed();

			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue { Values =new List<object>{  new DateTime(2017, 1, 1).ToUtc()},
						Comparison = Comparisons.Equal },
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "Effective"), false)
				}
			};

			var results = (await TableStorageRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(1);
			results[0].Id.ShouldBe("1");
		}

		[Fact]
		public async Task CanQueryWithSingleDateGreaterThan()
		{
			await Seed();

			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue { Values =new List<object>{  new DateTime(2019,1,1).ToUtc()},
						Comparison = Comparisons.GreaterThan },
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "Effective"), false)
				}
			};

			var results = (await TableStorageRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(2);
			results.Any(x => x.Id == "7").ShouldBeTrue();
			results.Any(x => x.Id == "8").ShouldBeTrue();
		}

		[Fact]
		public async Task CanQueryWithSingleDateGreaterEqualThan()
		{
			await Seed();

			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue { Values =new List<object>{  new DateTime(2019,1,1).ToUtc()},
						Comparison = Comparisons.GreaterEqualThan },
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "Effective"), false)
				}
			};

			var results = (await TableStorageRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(3);
			results.Any(x => x.Id == "6").ShouldBeTrue();
			results.Any(x => x.Id == "7").ShouldBeTrue();
			results.Any(x => x.Id == "8").ShouldBeTrue();
		}

		[Fact]
		public async Task CanQueryWithSingleDateLessEqualThan()
		{
			await Seed();

			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue { Values =new List<object>{  new DateTime(2016,1,1).ToUtc()},
						Comparison = Comparisons.LessThan },
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "Effective"), false)
				}
			};

			var results = (await TableStorageRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(2);
			results.Any(x => x.Id == "4").ShouldBe(true);
			results.Any(x => x.Id == "5").ShouldBe(true);
		}

		[Fact]
		public async Task CanQueryWithSingleGuidEquals()
		{
			await Seed();

			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue { Values =new List<object>{  Guid.Parse("9B522321-B689-43EC-A3DC-DC17EE2A42DD")},
						Comparison = Comparisons.Equal },
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "TypeId"), false)
				}
			};

			var results = (await TableStorageRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(2);
			results.Any(x => x.Id == "2").ShouldBe(true);
			results.Any(x => x.Id == "5").ShouldBe(true);
		}

		[Fact]
		public async Task CanQueryWithMultipleValueGuidsEquals()
		{
			await Seed();

			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue { Values =new List<object>
						{
							Guid.Parse("DE6EFD89-2D62-4E63-98D2-AEA9E622B223"),
							Guid.Parse("DC742320-F3A4-4DE1-9239-6A9EC68BD430"),
							Guid.Parse("CC562BC9-C2E5-46D8-B701-E5D57F5800B8")
						},
						Comparison = Comparisons.Equal },
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "TypeId"), false)
				}
			};

			var results = (await TableStorageRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(3);
			results.Any(x => x.Id == "8").ShouldBeTrue();
			results.Any(x => x.Id == "6").ShouldBeTrue();
			results.Any(x => x.Id == "3").ShouldBeTrue();
		}

		[Fact]
		public async Task CanQueryWithMultipleValueIntsEquals()
		{
			await Seed();

			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue { Values =new List<object>{ 8, 9 },
						Comparison = Comparisons.Equal },
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "Level"), false)
				}
			};

			var results = (await TableStorageRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(3);
			results.Any(x => x.Id == "6").ShouldBeTrue();
			results.Any(x => x.Id == "7").ShouldBeTrue();
			results.Any(x => x.Id == "8").ShouldBeTrue();
		}

		[Fact]
		public async Task CanQueryWithMultipleValueStringsEquals()
		{
			await Seed();

			QueryParameter[] args = {
				new QueryParameter
				{
					ContextValue = new ContextValue { Values =new List<object>{ "6","7","8" },
						Comparison = Comparisons.Equal },
					MemberModel = new ModelMember(_type, _accessor,
						_members.Single(x=>x.Name == "Id"), false)
				}
			};

			var results = (await TableStorageRepository.QueryAsync<DocumentDbFoo3>("test", args, null, null)).ToList();

			results.Count.ShouldBe(3);
			results.Any(x => x.Id == "6").ShouldBeTrue();
			results.Any(x => x.Id == "7").ShouldBeTrue();
			results.Any(x => x.Id == "8").ShouldBeTrue();
		}

		public void Dispose()
		{
			TableStorageRepository.DeleteAllAsync<DocumentDbFoo3>(null).GetAwaiter().GetResult();
		}
	}
}
