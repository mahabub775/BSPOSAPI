using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SMS.Core.Model;

public class CandidateQuizCompetitionModel : AuditModel
{
	public int CandidateQuizCompetitionId { get; set; }
	[DisplayName("Candidate")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Candidate'.")]
	public int CandidateID { get; set; }
	public DateTime CompetitionDate { get; set; }
	public string Name { get; set; }
	public int Number { get; set; }
	public string? Remarks { get; set; }
	public string? CreatedByName { get; set; }

}