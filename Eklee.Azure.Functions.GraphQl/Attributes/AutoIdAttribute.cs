using Eklee.Azure.Functions.GraphQl.Actions.AutoIdPatterns;
using System;

namespace Eklee.Azure.Functions.GraphQl.Attributes
{
	public class AutoIdAttribute : Attribute
	{
		public AutoIdAttribute(AutoIdPatterns autoIdPattern, Type type = null)
		{
			AutoIdPattern = autoIdPattern;

			if (AutoIdPattern == AutoIdPatterns.Guid)
			{
				Type = typeof(GuidAutoIdPattern);
			}
			else
			{
				Type = type;
			}

			if (Type == null)
				throw new InvalidOperationException("Type is required.");
		}

		public AutoIdPatterns AutoIdPattern { get; }

		public Type Type { get; }
	}
}
