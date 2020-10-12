﻿using System;

namespace Eklee.Azure.Functions.GraphQl.Tests
{
	public static class Extensions
	{
		public static DateTime ToUtc(this DateTime date)
		{
			return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0, DateTimeKind.Utc);
		}

		public static void Log(this string message)
		{
			Console.WriteLine($"[{DateTime.Now}] {message}");
		}
	}
}
