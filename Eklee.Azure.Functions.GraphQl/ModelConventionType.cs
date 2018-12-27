using System;
using GraphQL.Types;
using Microsoft.Extensions.Logging;

namespace Eklee.Azure.Functions.GraphQl
{
	public class ModelConventionType<TSourceType> : ObjectGraphType<TSourceType>
	{
		public ModelConventionType(ILogger logger)
		{
			try
			{
				logger.LogInformation($"Creating fields meta data for {typeof(TSourceType).FullName}");
				this.AddFields();
			}
			catch (Exception e)
			{
				logger.LogError(e, $"An error has occured while creating fields meta data for {typeof(TSourceType).FullName}.");
				throw;
			}
		}
	}
}
