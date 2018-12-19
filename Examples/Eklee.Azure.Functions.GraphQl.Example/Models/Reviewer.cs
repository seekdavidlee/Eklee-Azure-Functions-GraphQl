using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class Reviewer
	{
		[Key]
		[Description("Id of the Reviewer.")]
		public string Id { get; set; }

		[Description("Name of the reviewer.")]
		public string Name { get; set; }

		[Description("Different areas of interests of the reviewer.")]
		public List<string> Interests { get; set; }

		[Description("Region of where reviewer lives.")]
		public string Region { get; set; }
	}

	public class ReviewerId
	{
		[Key]
		[Description("Id of the Reviewer.")]
		public string Id { get; set; }
	}
}
