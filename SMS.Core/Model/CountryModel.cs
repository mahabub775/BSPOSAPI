using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SMS.Core.Model;

public class CountryModel : AuditModel
{
	public int CountryId { get; set; }

	[Required(ErrorMessage = "Please enter 'Name'.")]
	[MinLength(1, ErrorMessage = "Minimum length of 'Name' is 1 characters.")]
	[MaxLength(10, ErrorMessage = "Maximum length of 'Name' is 10 characters.")]
	public string Code { get; set; }

	[Required(ErrorMessage = "Please enter 'Name'.")]
	[MinLength(3, ErrorMessage = "Minimum length of 'Name' is 3 characters.")]
	[MaxLength(150, ErrorMessage = "Maximum length of 'Name' is 150 characters.")]
	public string Name { get; set; }
	

}