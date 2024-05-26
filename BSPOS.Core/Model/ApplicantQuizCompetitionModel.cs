using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SMS.Core.Model;

public class ApplicantQuizCompetitionModel : AuditModel
{
	public int ApplicantQuizCompetitionId { get; set; }
	[DisplayName("Applicant")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Applicant'.")]
	public int ApplicantID { get; set; }
	public DateTime CompetitionDate { get; set; }
	public string Name { get; set; }
	public int Number { get; set; }
	public string? Remarks { get; set; }
	public string? CreatedByName { get; set; }

}