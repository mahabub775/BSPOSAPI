using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SMS.Core.Model;

public class BrigadeModel : AuditModel
{
	public int BrigadeId { get; set; }

	[Required(ErrorMessage = "Please enter 'Name'.")]
	[MinLength(3, ErrorMessage = "Minimum length of 'Name' is 3 characters.")]
	[MaxLength(150, ErrorMessage = "Maximum length of 'Name' is 150 characters.")]
	public string BrigadeName { get; set; }
	public string? Description { get; set; }

}