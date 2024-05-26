using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SMS.Core.Model;

public class CandidateDisciplineModel : AuditModel
{
	public int CandidateDisciplineId { get; set; }

	[DisplayName("Candidate")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Candidate'.")]
	public int CandidateID { get; set; }
	[DisplayName("BAA Section")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'BAA Section'.")]
	public int BAASectionId { get; set; }
	public DateTime DisciplineDate { get; set; }
	
	public int PunishmentType { get; set; }
	public string? Remarks { get; set; }
	public string? BAASectionName { get; set; }

	public string? CreatedByName { get; set; }

}