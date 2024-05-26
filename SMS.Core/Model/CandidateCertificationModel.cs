using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SMS.Core.Model;

public class CandidateCertificationModel : AuditModel
{
	public int CandidateCertificationId { get; set; }

	[DisplayName("Candidate")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Candidate'.")]
	public int CandidateID { get; set; }

	[DisplayName("Certificate")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Certificate'.")]
	public int CertificateId { get; set; }

	[DisplayName("CertificateAuthority")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'CertificateAuthority'.")]
	public int CertificateAuthorityId { get; set; }

	[Required(ErrorMessage = "Please enter 'Certificate Number'.")]
	public string CertificateNumber { get; set; }
	
	public string? ImageUrl { get; set; }
	[DisplayName("Country")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Country'.")]
	public int CountryID { get; set; }

	[Range(1980, 2025, ErrorMessage = "Please select a 'Year'.")]
	public int Year { get; set; }
	public string? CreatedByName { get; set; }
	public string? CountryCode { get; set; }
	public string? CountryName { get; set; }
	public string? CertificateAuthorityName { get; set; }
	public string? CertificateName { get; set; }

	


}