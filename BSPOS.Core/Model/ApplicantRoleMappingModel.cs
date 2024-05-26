using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SMS.Core.Model;

public class ApplicantRoleMappingModel : AuditModel
{
	public int ApplicantRoleMappingId { get; set; }

	[DisplayName("Unit")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Unit'.")]
	public int UnitID { get; set; }

	[DisplayName("Brigade")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Brigade'.")]
	public int BrigadeID { get; set; }

	[DisplayName("Company")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Company'.")]
	public int CompanyID { get; set; }
	[DisplayName("Platoon")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Platoon'.")]
	public int PlatoonID { get; set; }
	public string? UnitName { get; set; }
	public string? BrigadeName { get; set; }
	public string? CompanyName { get; set; }
	public string? PlatoonName { get; set; }

}