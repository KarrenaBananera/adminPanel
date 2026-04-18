using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using tableRazorAssigment.Data;
using tableRazorAssigment.Services;

namespace tableRazorAssigment.Middleware;

public class LogOutInvalidUserMiddleware
{
    private readonly RequestDelegate _next;

    public LogOutInvalidUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
    {
        if (await IsUserValidAsync(context, serviceProvider) == false)
        {
            await context.SignOutAsync(IdentityConstants.ApplicationScheme);
        }
        await _next(context);
    }

    private async Task<bool> IsUserValidAsync(HttpContext context, IServiceProvider serviceProvider)
    {
        if (context.User.Identity?.IsAuthenticated == false)
            return true;
        var user = await GetUserInScopeAsync(context, serviceProvider);
        if (user == null || user.IsUserBlocked)
            return false;
        return true;
    }

    private async Task<User?> GetUserInScopeAsync(HttpContext context, IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var currentUserService = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
        return await currentUserService.GetCurrentUserAsync(context);
    }

}
