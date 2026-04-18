using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using tableRazorAssigment.Data;

namespace tableRazorAssigment.Services.Implenetation;

public class CurrentUserService : ICurrentUserService
{
    private readonly UserManager<User> userManager;
    User? _currentUser;
    bool _checked = false;

    public CurrentUserService(UserManager<User> userManager)
    {
        this.userManager = userManager;
    }

    public async Task<User?> GetCurrentUserAsync(HttpContext context)
    {
        if (_checked == true)
            return _currentUser;
        var id = GetUserId(context);
        return await userManager.FindByIdAsync(id);
    }

    private string? GetUserId(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == false)
            return null;
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return null;
        return userIdClaim.Value;
    }

}
