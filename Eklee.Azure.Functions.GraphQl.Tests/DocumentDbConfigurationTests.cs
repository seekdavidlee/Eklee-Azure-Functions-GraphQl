using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Eklee.Azure.Functions.GraphQl.Repository;
using Eklee.Azure.Functions.Http;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests
{
	public class DocumentDbFoo1
	{
		[Key]
		public string Id { get; set; }

		public string Name { get; set; }

		public string MyStringCategory { get; set; }
	}

	public class DocumentDbFoo2
	{
		[Key]
		public string Id { get; set; }

		public string Name { get; set; }

		public int MyIntCategory { get; set; }
	}

	/// <summary>
	/// Used mainly for comparisons.
	/// </summary>
	public class DocumentDbFoo3
	{
		[Key]
		public string Id { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }

		public int Level { get; set; }

		public DateTime Effective { get; set; }

		public DateTime? Expires { get; set; }

		public string Category { get; set; }
	}

	[Trait(Constants.Category, Constants.UnitTests)]
	public class DocumentDbConfigurationTests
	{
		[Fact]
		public void BuildWithoutPartition_GetInvalidOperationException()
		{
			var builder = Substitute.For<IModelConventionInputBuilder<DocumentDbFoo1>>();
			var repo = Substitute.For<IGraphQlRepository>();
			var ctx = Substitute.For<IHttpRequestContext>();
			var config = new DocumentDbConfiguration<DocumentDbFoo1>(builder, repo, typeof(DocumentDbFoo1), ctx);

			Should.Throw<InvalidOperationException>(() => config.BuildDocumentDb());
		}

		[Fact]
		public void BuildWithStringPartition_GetParitionConfigurationAsInput()
		{
			var builder = Substitute.For<IModelConventionInputBuilder<DocumentDbFoo1>>();
			var repo = Substitute.For<IGraphQlRepository>();
			var ctx = Substitute.For<IHttpRequestContext>();
			var config = new DocumentDbConfiguration<DocumentDbFoo1>(builder, repo, typeof(DocumentDbFoo1), ctx);

			config.AddPartition(x => x.Name).BuildDocumentDb();

			repo.Received(1).Configure(Arg.Any<Type>(),
				Arg.Is<Dictionary<string, object>>(
					x => x.Keys.Count(k => k.Contains(DocumentDbConstants.PartitionMemberExpression)) == 1));
		}

		[Fact]
		public void BuildWithIntPartition_GetParitionConfigurationAsInput()
		{
			var builder = Substitute.For<IModelConventionInputBuilder<DocumentDbFoo2>>();
			var repo = Substitute.For<IGraphQlRepository>();
			var ctx = Substitute.For<IHttpRequestContext>();
			var config = new DocumentDbConfiguration<DocumentDbFoo2>(builder, repo, typeof(DocumentDbFoo2), ctx);

			config.AddPartition(x => x.MyIntCategory).BuildDocumentDb();

			repo.Received(1).Configure(Arg.Any<Type>(),
				Arg.Is<Dictionary<string, object>>(
					x => x.Keys.Count(k => k.Contains(DocumentDbConstants.PartitionMemberExpression)) == 1));
		}
	}
}
