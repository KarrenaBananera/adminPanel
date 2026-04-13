using Microsoft.AspNetCore.Identity;

namespace tableRazorAssigment.Data;

public class User : IdentityUser
{
    public string? Title { get; set; }
    public string UserEmail { get; set; }
    public UserStatus Status { get; set; }
    public DateTime LastSeen { get; set; }
}
