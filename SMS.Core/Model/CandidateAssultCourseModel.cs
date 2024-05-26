using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SMS.Core.Model;

public class CandidateAssultCourseModel : AuditModel
{
	public int CandidateAssultCourseId { get; set; }

	[DisplayName("Candidate")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Candidate'.")]
	public int CandidateID { get; set; }
	public DateTime CourseDate { get; set; }
	public TimeSpan CourseTime { get; set; }
	public int Mark { get; set; }
	public string? Remarks { get; set; }
	public string? CreatedByName { get; set; }

}