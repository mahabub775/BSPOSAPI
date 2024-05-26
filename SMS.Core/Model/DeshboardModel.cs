using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SMS.Core.Model;

public class DeshboardModel : AuditModel
{
	public int AdminUser { get; set; }
	public int NormalUser { get; set; }
	public int SAUser { get; set; }
	public int TotalUser { get; set; }
	public int TotalUnit { get; set; }
	public int TotalCompany { get; set; }
	public int TotalPlatoon { get; set; }

}