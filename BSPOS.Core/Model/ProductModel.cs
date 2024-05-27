using System.ComponentModel.DataAnnotations;

namespace BSPOS.Core.Model;

public class ProductModel : AuditModel
{
	public int Id { get; set; }
	public string Code { get; set; }

	[Required(ErrorMessage = "Please enter 'Name'.")]
	[MinLength(3, ErrorMessage = "Minimum length of 'Name' is 3 characters.")]
	[MaxLength(150, ErrorMessage = "Maximum length of 'Name' is 150 characters.")]
	public string Name { get; set; }

	public double Price { get; set; }

	
}