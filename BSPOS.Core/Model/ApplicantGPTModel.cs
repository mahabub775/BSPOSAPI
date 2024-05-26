using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SMS.Core.Model;

public class ApplicantGPTModel : AuditModel
{
	public int ApplicantGPTId { get; set; }

	[DisplayName("Applicant")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Applicant'.")]
	public int ApplicantID { get; set; }
	public DateTime PostingDate { get; set; }
	public double WT1st { get; set; }
	public double WT2nd { get; set; }
	public double WT3rd { get; set; }
	public double WT4th { get; set; }
	public double WT5th { get; set; }
	public double WT6th { get; set; }
	public double WTTotalMk { get; set; }
	public double WTTotalWt { get; set; }
	public double WH1st { get; set; }
	public double WH2nd { get; set; }
	public double WH3rd { get; set; }
	public double WH4th { get; set; }
	public double WHTotalMk { get; set; }
	public double WHTotalWt { get; set; }
	public double STX { get; set; }
	public double STX2 { get; set; }
	public double STX3 { get; set; }
	public double STX4 { get; set; }
	public double STX5 { get; set; }
	public double STX6 { get; set; }
	public double STX7 { get; set; }
	public double STX8 { get; set; }
	public double STX9 { get; set; }
	public double STXTotalMk { get; set; }
	public double STXTotalWt { get; set; }
	public double PracParts { get; set; }
	public double PracETS { get; set; }
	public double PracCC { get; set; }
	public double PracSalutingTest { get; set; }
	public double PracTotalMk { get; set; }
	public double PracTotalWt { get; set; }
	public double FEWritten { get; set; }
	public double FEPrac { get; set; }
	public double FETotalMk { get; set; }
	public double FETotalWt { get; set; }
	public double CEETotalMk { get; set; }
	public double CEETotalWt { get; set; }
	public double AdminGenAware { get; set; }
	public double AdminDecipline { get; set; }
	public double AdminTotalMk { get; set; }
	public double AdminTotalWt { get; set; }
	public double GrandTotalMk { get; set; }
	public double GrandTotalWt { get; set; }
	public string Remarks { get; set; }

	public string? CreatedByName { get; set; }
	public string? Name { get; set; }



}