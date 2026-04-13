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
        context.Response.Redirect("/Account/Login");
    }

    private async Task UpdateLastSeen(User? user, HttpContext context, ApplicationDbContext dbContext)
    {
        if (user is null)
            return;
        var now = DateTime.UtcNow;
        user.LastSeen = now;
        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync();
        return;
    }

    public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
    {
        var userId = GetUserId(context);
        if (userId is null)
        {
            await _next(context);
            return;
        }
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await dbContext.Users.FindAsync(userId);
        await LogOutDeletedUser(user, context);
        await UpdateLastSeen(user, context, dbContext);
        await _next(context);
    }

}
