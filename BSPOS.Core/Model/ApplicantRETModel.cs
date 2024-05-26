using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SMS.Core.Model;

public class ApplicantRETModel : AuditModel
{
	public int ApplicantRETId { get; set; }
	[DisplayName("Applicant")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Applicant'.")]
	public int ApplicantID { get; set; }
	public DateTime RETDate { get; set; }
	public int Result { get; set; }
	public int Mark { get; set; }
	public string? Remarks { get; set; }
	public string? BIAnnualName { get; set; }

	public string? CreatedByName { get; set; }

}