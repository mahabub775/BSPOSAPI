using Microsoft.AspNetCore.Identity;

namespace SMS.API.Persistence.Identity;

public class ApplicationUser : IdentityUser
{
	public string FullName { get; set; }
}