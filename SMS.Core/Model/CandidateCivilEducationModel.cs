using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SMS.Core.Model;

public class CandidateCivilEducationModel : AuditModel
{
	public int CandidateCivilEducationId { get; set; }

	[DisplayName("Candidate")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Candidate'.")]
	public int CandidateID { get; set; }

	[DisplayName("Degree")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Degree'.")]
	public int DegreeID { get; set; }

	[DisplayName("Institution")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Institution'.")]
	public int InstitutionID { get; set; }

	[Range(0, 5, ErrorMessage = "Please Type  'Result'.")]
	public decimal Result { get; set; }

	[DisplayName("Passing Year")]
	[Range(1980, 2025, ErrorMessage = "Please select a 'Year Of Passing'.")]
	public int YearOfPassing { get; set; }

	[DisplayName("Duration")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Duration'.")]
	public int DurationID { get; set; }


	public string? DurationName { get; set; }
	public string? InstitutionName { get; set; }
	public string? DegreeName { get; set; }



	public string? CreatedByName { get; set; }

}