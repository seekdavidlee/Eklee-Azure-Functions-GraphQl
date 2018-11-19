using System.ComponentModel;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class Status
	{
		[Description("Message describing the status of the request.")]
		public string Message { get; set; }
	}
}
