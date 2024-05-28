using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSPOS.API.Persistence.Identity;

public class ApplicationUser : IdentityUser
{
	public string FullName { get; set; }
	public string Address { get; set; }
	[NotMapped]
	public string RoleName {  get; set; }
}