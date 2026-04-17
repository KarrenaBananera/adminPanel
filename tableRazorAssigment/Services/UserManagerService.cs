
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using tableRazorAssigment.Data;

namespace tableRazorAssigment.Services;

public class UserManagerService : IUserManagerService
{
    private readonly ApplicationDbContext _dbContext;

    public UserManagerService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task BLockUsersAsync(List<string> userIds)
    {
        await _dbContext.Users
            .Where(u => userIds.Contains(u.Id))
            .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.IsUserBlocked, true));
    }

    public async Task DeleteUsersAsync(List<string> userIds)
    {
        await _dbContext.Users
            .Where(u => userIds.Contains(u.Id))
            .ExecuteDeleteAsync();
    }

    public async Task DeleteUnverifiedAsync(List<string> userIds)
    {
        await _dbContext.Users
           .Where(u => userIds.Contains(u.Id))
           .Where(u => u.IsUserEmailConfirmed == false)
           .ExecuteDeleteAsync();
    }

    public async Task UnblockUsersAsync(List<string> userIds)
    {
        await _dbContext.Users
           .Where(u => userIds.Contains(u.Id))
           .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.IsUserBlocked, false));
    }

}
