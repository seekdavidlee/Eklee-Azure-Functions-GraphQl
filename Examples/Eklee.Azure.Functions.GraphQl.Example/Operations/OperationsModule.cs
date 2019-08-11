using Autofac;

namespace Eklee.Azure.Functions.GraphQl.Example.Operations
{
	public class OperationsModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<Operations>().As<IOperations>().SingleInstance();
		}
	}
}
