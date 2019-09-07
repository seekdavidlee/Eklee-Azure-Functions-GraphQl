using Eklee.Azure.Functions.GraphQl.Actions;
using Eklee.Azure.Functions.GraphQl.Repository;
using FastMember;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests.Actions
{
	public class MyModelTransformer : IModelTransformer
	{
		public MyModelTransformer()
		{
			List1 = new List<MockModel1>();
			List2 = new List<MockModel2>();
			List3 = new List<MockModel3>();
			List4 = new List<MockModel4>();
		}

		public int ExecutionOrder => 0;

		public bool CanHandle(MutationActions action)
		{
			return action == MutationActions.BatchCreate;
		}

		public Task TransformAsync(object item, TypeAccessor typeAccessor, IGraphRequestContext context)
		{
			if (item.GetType() == typeof(MockModel1))
			{
				List1.Add((MockModel1)item);
			}

			if (item.GetType() == typeof(MockModel2))
			{
				List2.Add((MockModel2)item);
			}

			if (item.GetType() == typeof(MockModel3))
			{
				List3.Add((MockModel3)item);
			}

			if (item.GetType() == typeof(MockModel4))
			{
				List4.Add((MockModel4)item);
			}
			return Task.CompletedTask;
		}

		public List<MockModel1> List1 { get; set; }
		public List<MockModel2> List2 { get; set; }
		public List<MockModel3> List3 { get; set; }
		public List<MockModel4> List4 { get; set; }
	}

	[Trait(Constants.Category, Constants.UnitTests)]
	public class ModelTransformerProviderTests
	{
		private readonly MyModelTransformer _modelTransformer;
		private readonly IModelTransformerProvider _modelTransformerProvider;

		public ModelTransformerProviderTests()
		{
			_modelTransformer = new MyModelTransformer();
			_modelTransformerProvider = new ModelTransformerProvider(new List<IModelTransformer> { _modelTransformer });
		}

		[Fact]
		public async Task CanHandleNulllListTypes()
		{
			var mock1 = new MockModel1
			{
				Id = "a1",
			};

			await _modelTransformerProvider.TransformAsync(new ModelTransformArguments
			{
				Action = MutationActions.BatchCreate,
				Models = new List<object> { mock1 }
			});

			_modelTransformer.List1.Count.ShouldBe(1);
			_modelTransformer.List1.SingleOrDefault(x => x.Id == "a1").ShouldNotBeNull();
		}

		[Fact]
		public async Task CanHandleNestedListTypes()
		{
			var mock1 = new MockModel1
			{
				Id = "a1",
				Model2List = new List<MockModel2>
				{
					new MockModel2
					{
						Id = "b1a",
						Model3List = new List<MockModel3>
						{
							new MockModel3
							{
								Id = "c1a",
								Value = "c1a"
							}
						},
						SomeMock = new MockModel4
						{
							Id="d1a",
							Value  = "d1a"
						}
					},
					new MockModel2
					{
						Id = "b1b",
						Model3List = new List<MockModel3>
						{
							new MockModel3
							{
								Id = "c1b",
								Value = "c1b"
							}
						},
						SomeMock = new MockModel4
						{
							Id="d1b",
							Value  = "d1b"
						}
					}
				}
			};

			var mock2 = new MockModel1
			{
				Id = "a2",
				Model2List = new List<MockModel2>
				{
					new MockModel2
					{
						Id = "b2",
						Model3List = new List<MockModel3>
						{
							new MockModel3
							{
								Id = "c2",
								Value = "c2"
							}
						}
					}
				}
			};

			await _modelTransformerProvider.TransformAsync(new ModelTransformArguments
			{
				Action = MutationActions.BatchCreate,
				Models = new List<object> { mock1, mock2 }
			});

			_modelTransformer.List1.Count.ShouldBe(2);
			_modelTransformer.List1.SingleOrDefault(x => x.Id == "a1").ShouldNotBeNull();
			_modelTransformer.List1.SingleOrDefault(x => x.Id == "a2").ShouldNotBeNull();

			_modelTransformer.List2.Count.ShouldBe(3);
			_modelTransformer.List2.SingleOrDefault(x => x.Id == "b1a").ShouldNotBeNull();
			_modelTransformer.List2.SingleOrDefault(x => x.Id == "b1b").ShouldNotBeNull();
			_modelTransformer.List2.SingleOrDefault(x => x.Id == "b2").ShouldNotBeNull();

			_modelTransformer.List3.Count.ShouldBe(3);
			_modelTransformer.List3.SingleOrDefault(x => x.Id == "c1a").ShouldNotBeNull();
			_modelTransformer.List3.SingleOrDefault(x => x.Id == "c1b").ShouldNotBeNull();
			_modelTransformer.List3.SingleOrDefault(x => x.Id == "c2").ShouldNotBeNull();

			_modelTransformer.List4.Count.ShouldBe(2);
			_modelTransformer.List4.SingleOrDefault(x => x.Id == "d1a").ShouldNotBeNull();
			_modelTransformer.List4.SingleOrDefault(x => x.Id == "d1b").ShouldNotBeNull();

		}
	}
}
