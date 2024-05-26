using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SMS.Core.Model;

public class DeshboardModel : AuditModel
{
	public int BrigadeUser { get; set; }
	public int UnitUser { get; set; }
	public int CompanyUser { get; set; }
	public int PlatoonUser { get; set; }
	public int Soldier { get; set; }

	//BrigadeUser, UnitUser, CompanyUser, PlatoonUser, Soldier
	public int SAUser { get; set; }
	public int TotalUser { get; set; }
	public int TotalUnit { get; set; }
	public int TotalCompany { get; set; }
	public int TotalPlatoon { get; set; }

}