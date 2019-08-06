using System.ComponentModel;

namespace Eklee.Azure.Functions.GraphQl.Example.Models
{
	public class Model3V2 : Model3
	{
		[Description("FieldTwo")]
		public string FieldTwo { get; set; }

		[Description("FieldThree")]
		public string FieldThree { get; set; }
	}
}