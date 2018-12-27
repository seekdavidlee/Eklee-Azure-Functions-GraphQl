using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.BusinessLayer
{
	public class BookPriceId
	{
		[Key]
		[Description("Id of book price.")]
		public Guid Id { get; set; }
	}

	public class BookPrice
	{
		[Key]
		[Description("Id of book price.")]
		public Guid Id { get; set; }

		[Description("Description.")]
		public string Description { get; set; }

		[Description("Description.")]
		public string Type { get; set; }
	}
}
