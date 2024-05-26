using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SMS.Core.Model;

public class ApplicantIPFTModel : AuditModel
{
	public int ApplicantIPFTId { get; set; }

	[DisplayName("Applicant")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Applicant'.")]
	public int ApplicantID { get; set; }
	[DisplayName("BI Annual")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'BI Annual'.")]
	public int BIAnnualId { get; set; }
	public DateTime IPFTDate { get; set; }
	public int Result { get; set; }
	public int Attempt { get; set; }
	public string? Remarks { get; set; }
	public string? BIAnnualName { get; set; }

	public string? CreatedByName { get; set; }

}