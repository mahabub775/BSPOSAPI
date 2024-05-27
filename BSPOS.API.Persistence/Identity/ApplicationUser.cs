using Microsoft.AspNetCore.Identity;

namespace BSPOS.API.Persistence.Identity;

public class ApplicationUser : IdentityUser
{
	public string FullName { get; set; }
}