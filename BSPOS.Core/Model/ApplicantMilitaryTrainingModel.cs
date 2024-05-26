using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SMS.Core.Model;

public class ApplicantMilitaryTrainingModel : AuditModel
{
	public int ApplicantMilitaryTrainingId { get; set; }

	[DisplayName("Applicant")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Applicant'.")]
	public int ApplicantID { get; set; }

	[DisplayName("Training")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Training'.")]
	public int TrainingID { get; set; }
	[DisplayName("Duration")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Duration'.")]
	public int DurationID { get; set; }
	[Required(ErrorMessage = "Please enter 'Result'.")]
	public string Result { get; set; }

	[DisplayName("Training Year")]
	[Range(1980, 2025, ErrorMessage = "Please select a 'Year Of Training'.")]
	public int YearOfTraining { get; set; }

	public string? DurationName { get; set; }
	public string? TrainingName { get; set; }

	public string? CreatedByName { get; set; }

}