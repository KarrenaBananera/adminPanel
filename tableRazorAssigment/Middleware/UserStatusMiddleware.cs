using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using tableRazorAssigment.Data;

namespace tableRazorAssigment.Middleware;

public class UserStatusMiddleware
{
    private readonly RequestDelegate _next;

    public UserStatusMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
    {
        var userId = GetUserId(context);
        if (userId is null)
        {
            await _next(context);
            return;
        }
        await ScopeActionsAsync(userId, context, serviceProvider);
        await _next(context);
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

    private async Task LogOutDeletedUser(User? user, HttpContext context)
    {
        if (user is not null)
            return;
        await context.SignOutAsync(IdentityConstants.ApplicationScheme);
        if (CheckNotIndex(context))
            context.Response.Redirect("/Index");
    }

    private async Task LogOutBlockedUser(User? user, HttpContext context)
    {
        if (user is null || user.IsUserBlocked == false)
            return;
        await context.SignOutAsync(IdentityConstants.ApplicationScheme);
    }

    private bool CheckNotIndex(HttpContext context)
    {
        if (context.Request.Path.Equals("/Index", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        return true;
    }

    private async Task UpdateLastSeen(User? user, UserManager<User> userManager)
    {
        if (user is null)
            return;
        var now = DateTime.UtcNow;
        user.LastSeen = now;
        await userManager.UpdateAsync(user);
        return;
    }

    private async Task ScopeActionsAsync(string userId, HttpContext context, IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        await UpdateUserAsync(userId, userManager, context);
    }

    private async Task UpdateUserAsync(string userId, UserManager<User> userManager, HttpContext context)
    {
        var user = await userManager.FindByIdAsync(userId);
        await LogOutDeletedUser(user, context);
        await UpdateLastSeen(user, userManager);
    }

}
