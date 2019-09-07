using System;

namespace Eklee.Azure.Functions.GraphQl.Attributes
{
	public enum AutoDateTimeTypes
	{
		UtcToday,
		UtcNow
	}

	public class AutoDateTimeAttribute : Attribute
	{
		public AutoDateTimeAttribute(AutoDateTimeTypes autoDateTimeTypes)
		{
			AutoDateTimeTypes = autoDateTimeTypes;
		}

		public AutoDateTimeTypes AutoDateTimeTypes { get; }
	}
}
