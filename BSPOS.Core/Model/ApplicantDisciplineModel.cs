using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SMS.Core.Model;

public class ApplicantDisciplineModel : AuditModel
{
	public int ApplicantDisciplineId { get; set; }

	[DisplayName("Applicant")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Applicant'.")]
	public int ApplicantID { get; set; }
	[DisplayName("BAA Section")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'BAA Section'.")]
	public int BAASectionId { get; set; }
	public DateTime DisciplineDate { get; set; }
	
	public int PunishmentType { get; set; }
	public string? Remarks { get; set; }
	public string? BAASectionName { get; set; }

	public string? CreatedByName { get; set; }

}