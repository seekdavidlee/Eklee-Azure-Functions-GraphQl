using Autofac;
using Eklee.Azure.Functions.GraphQl.Example.Validations;
using Eklee.Azure.Functions.GraphQl.Validations;
using Eklee.Azure.Functions.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace Eklee.Azure.Functions.GraphQl.Example.TestDocumentDbWithPermissions
{
	public class TestDocumentDbWithPermissionsModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.UseDistributedCache<MemoryDistributedCache>();
			builder.UseJwtAuthorization<JwtConfigParameters>();
			builder.RegisterGraphQl<TestDocumentDbWithPermissionsSchema>();
			builder.RegisterType<TestDocumentDbWithPermissionsQuery>();
			builder.RegisterType<TestDocumentDbWithPermissionsMutation>();
			builder.UseDataAnnotationsValidation();
			builder.RegisterType<MyValidation>().As<IModelValidation>();
		}
	}
}
