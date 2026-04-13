using Microsoft.AspNetCore.Identity;

namespace tableRazorAssigment.Data;

public class User : IdentityUser
{
    public string? Title { get; set; }
    public string Name { get; set; }
    public string UserEmail { get; set; }
    public bool IsUserBlocked { get; set; }
    public bool IsUserEmailConfirmed { get; set; }
    public DateTime LastSeen { get; set; }
}
