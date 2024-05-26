using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SMS.Core.Model;

public class CandidateModel : AuditModel
{
	public int CandidateId { get; set; }
	[Required(ErrorMessage = "Please enter 'Candidate Name'.")]
	[MinLength(3, ErrorMessage = "Minimum length of 'CandidateName' is 3 characters.")]
	[MaxLength(150, ErrorMessage = "Maximum length of 'Candidate Name' is 150 characters.")]
	public string CandidateName { get; set; }
	[Required(ErrorMessage = "Please enter 'ArmyNo'.")]
	[MinLength(3, ErrorMessage = "Minimum length of 'ArmyNo' is 3 characters.")]
	[MaxLength(6, ErrorMessage = "Maximum length of 'ArmyNo' is 6 characters.")]
	public string ArmyNo { get; set; }

	[DisplayName("Rank")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Rank'.")]
	public int RankID { get; set; }

	[DisplayName("Trade")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Trade'.")]
	public int TradeID { get; set; }
	[DisplayName("Unit")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Unit'.")]
	public int UnitID { get; set; }

	[DisplayName("Brigade")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Brigade'.")]
	public int BrigadeID { get; set; }

	[DisplayName("Company")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Company'.")]
	public int CompanyID { get; set; }
	[DisplayName("Platoon")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Platoon'.")]
	public int PlatoonID { get; set; }
	public string Mobile { get; set; }
	public string Email { get; set; }
	public string UserId { get; set; }
	public string? ImageUrl { get; set; }
	public string PostedDate { get; set; }
	public bool Active { get; set; }

	public string? RankName { get; set; }
	public string? TradeName { get; set; }
	public string? UnitName { get; set; }
	public string? BrigadeName { get; set; }
	public string? CompanyName { get; set; }
	public string? PlatoonName { get; set; }
	public string? UserName { get; set; }
	public string? CreatedByName { get; set; }


	#region report property
	public string? CourseName { get; set; }
	public decimal? CadreResult { get; set; }

	public decimal? TotalMks { get; set; }
	public string? GPTRemarks { get; set; }
	public string? DegreeName { get; set; }
	public decimal? CivEduResult { get; set; }
	#endregion
}