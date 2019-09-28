using Eklee.Azure.Functions.GraphQl.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class Model13Child
	{
		[Key]
		[Description("Id of destination")]
		public string ChildId { get; set; }

		[Description("Field")]
		public string Field { get; set; }

		[PartitionKey]
		[Description("AccountId")]
		public string AccountId { get; set; }
	}
}
