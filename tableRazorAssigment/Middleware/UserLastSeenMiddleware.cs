using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using tableRazorAssigment.Data;
using tableRazorAssigment.Services;

namespace tableRazorAssigment.Middleware;

public class UserLastSeenMiddleware
{
    private readonly RequestDelegate _next;

    public UserLastSeenMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
    {
        await ScopeActionsAsync(context, serviceProvider);
        await _next(context);
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

    private async Task ScopeActionsAsync(HttpContext context, IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var currentUserService = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
        await UpdateUserAsync(userManager, context, currentUserService);
    }

    private async Task UpdateUserAsync(UserManager<User> userManager, HttpContext context, ICurrentUserService currentUserService)
    {
        var user = await currentUserService.GetCurrentUserAsync(context);
        await UpdateLastSeen(user, userManager);
    }

}
