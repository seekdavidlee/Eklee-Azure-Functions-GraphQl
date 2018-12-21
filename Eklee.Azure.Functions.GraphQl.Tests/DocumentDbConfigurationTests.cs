using System;
using Eklee.Azure.Functions.GraphQl.Repository;
using Eklee.Azure.Functions.Http;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Eklee.Azure.Functions.GraphQl.Tests
{
	public class Foo
	{
		public string Name { get; set; }
	}
	public class DocumentDbConfigurationTests
	{
		[Fact]
		public void BuildWithoutPartition_GetInvalidOperationException()
		{
			var builder = Substitute.For<IModelConventionInputBuilder<Foo>>();
			var repo = Substitute.For<IGraphQlRepository>();
			var ctx = Substitute.For<IHttpRequestContext>();
			var config = new DocumentDbConfiguration<Foo>(builder, repo, typeof(Foo), ctx);

			Should.Throw<InvalidOperationException>(() => config.BuildDocumentDb());
		}
	}
}
