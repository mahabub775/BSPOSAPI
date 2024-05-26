using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SMS.Core.Model;

public class ApplicantAssultCourseModel : AuditModel
{
	public int ApplicantAssultCourseId { get; set; }

	[DisplayName("Applicant")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Applicant'.")]
	public int ApplicantID { get; set; }
	public DateTime CourseDate { get; set; }
	public TimeSpan CourseTime { get; set; }
	public int Mark { get; set; }
	public string? Remarks { get; set; }
	public string? CreatedByName { get; set; }

}