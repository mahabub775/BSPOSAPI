using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SMS.Core.Model;

public class CandidateCadreModel : AuditModel
{
	public int CandidateCadreId { get; set; }

	[DisplayName("Candidate")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Candidate'.")]
	public int CandidateID { get; set; }
	
	[DisplayName("Course")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Course'.")]
	public int CourseID { get; set; }
	
	[DisplayName("Institution")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Institution'.")]
	public int InstitutionID { get; set; }
	[Range(0, 5, ErrorMessage = "Result must be between 0 and 5")]
	public decimal Result { get; set; }

	public string? CreatedByName { get; set; }
	public string? InstitutionName { get; set; }
	public string? CourseName { get; set; }

	


}