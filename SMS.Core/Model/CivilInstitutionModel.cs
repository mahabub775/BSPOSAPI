using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SMS.Core.Model;

public class CivilInstitutionModel : AuditModel
{
	public int CivilInstitutionId { get; set; }

	[Required(ErrorMessage = "Please enter 'Name'.")]
	[MinLength(3, ErrorMessage = "Minimum length of 'Name' is 3 characters.")]
	[MaxLength(150, ErrorMessage = "Maximum length of 'Name' is 150 characters.")]
	public string InstitutionName { get; set; }
	public string? Description { get; set; }
	public int? UseInstitution { get; set; }

}