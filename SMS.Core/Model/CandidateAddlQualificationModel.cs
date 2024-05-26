using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SMS.Core.Model;

public class CandidateAddlQualificationModel : AuditModel
{
	public int CandidateAddlQualificationId { get; set; }

	[DisplayName("Candidate")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Candidate'.")]
	public int CandidateID { get; set; }
	
	[DisplayName("Qualification")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Qualification'.")]
	public int QualificationID { get; set; }

	public string? ImageUrl { get; set; }
	public string? Description { get; set; }
	public string? CreatedByName { get; set; }
	public string? QualificationName { get; set; }
	

	


}