using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class BookId
	{
		[Description("Id of book.")]
		[Key]
		public string Id { get; set; }
	}
}
