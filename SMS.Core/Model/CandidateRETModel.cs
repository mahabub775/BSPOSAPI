using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SMS.Core.Model;

public class CandidateRETModel : AuditModel
{
	public int CandidateRETId { get; set; }
	[DisplayName("Candidate")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Candidate'.")]
	public int CandidateID { get; set; }
	public DateTime RETDate { get; set; }
	public int Result { get; set; }
	public int Mark { get; set; }
	public string? Remarks { get; set; }
	public string? BIAnnualName { get; set; }

	public string? CreatedByName { get; set; }

}