using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using tableRazorAssigment.Data;
using tableRazorAssigment.Model;
using tableRazorAssigment.Services;

namespace tableRazorAssigment.Pages.Api;

[Authorize]
public class UsersModel : PageModel
{
    private readonly IUserFetcher _userFetcher;
    private readonly IUserManagerService _userManagerService;
    private readonly UserManager<User> _userManager;
    private User? _currentUser;
    public UsersModel(IUserFetcher userFetcher, IUserManagerService userManagerService, UserManager<User> userManager)
    {
        _userFetcher = userFetcher;
        _userManagerService = userManagerService;
        _userManager = userManager;
    }

    public async Task<IActionResult> OnGet(UserQueryParameters parameters)
    {
        var totalCount = await _userFetcher.TotalCountAsync(parameters);
        var users = await _userFetcher.FetchUsersAsync(parameters);
        return new JsonResult(new
        {
            Items = users,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)parameters.PageSize)
        });
    }

    public async Task<IActionResult> OnGetCurrentUser()
    {
        var user = await _userFetcher.GetCurrentUserAsync(HttpContext);
        return new JsonResult(new
        {
            Item = user
        });
    }

    public async Task<IActionResult> OnPostDelete([FromBody] List<string> userIds)
    {
        if (await CheckInvalidRequestAsync(userIds))
            return BadRequest("No user IDs provided.");
        await _userManagerService.DeleteUsersAsync(userIds);
        return new OkResult();
    }

    public async Task<IActionResult> OnPostBlock([FromBody] List<string> userIds)
    {
        if (await CheckInvalidRequestAsync(userIds))
            return BadRequest("No user IDs provided.");
        await _userManagerService.BLockUsersAsync(userIds);
        return new OkResult();
    }

    public async Task<IActionResult> OnPostUnblock([FromBody] List<string> userIds)
    {
        if (await CheckInvalidRequestAsync(userIds))
            return BadRequest("No user IDs provided.");
        await _userManagerService.UnblockUsersAsync(userIds);
        return new OkResult();
    }

    public async Task<IActionResult> OnPostDeleteUnverified([FromBody] List<string> userIds)
    {
        if (await CheckInvalidRequestAsync(userIds))
            return BadRequest("No user IDs provided.");
        await _userManagerService.DeleteUnverifiedAsync(userIds);
        return new OkResult();
    }

    private async Task<bool> CheckInvalidRequestAsync(List<string> userIds)
    {
        if (userIds == null || !userIds.Any() || await CheckUserBlockedAsync() == true)
            return true;
        return false;
    }

    private async Task<bool?> CheckUserBlockedAsync()
    {
        var currentUser = await GetCurrentUserAsync();
        if (_currentUser == null)
            return null;
        return currentUser.IsUserBlocked;
    }

    private async Task<User?> GetCurrentUserAsync()
    {
        if (_currentUser is null)
        {
            var userId = _userManager.GetUserId(User);
            if (userId != null)
                _currentUser = await _userManager.FindByIdAsync(userId);
        }
        return _currentUser;
    }
}
