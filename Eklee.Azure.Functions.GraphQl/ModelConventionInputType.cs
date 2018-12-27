using System;
using GraphQL.Types;
using Microsoft.Extensions.Logging;

namespace Eklee.Azure.Functions.GraphQl
{
	public class ModelConventionInputType<TSourceType> : InputObjectGraphType<TSourceType>
	{
		public ModelConventionInputType(ILogger logger)
		{
			try
			{
				logger.LogInformation($"Creating input fields meta data for {typeof(TSourceType).FullName}");
				this.AddFields();
			}
			catch (Exception e)
			{
				logger.LogError(e, $"An error has occured while creating input fields meta data for {typeof(TSourceType).FullName}.");
				throw;
			}
		}
	}
}
