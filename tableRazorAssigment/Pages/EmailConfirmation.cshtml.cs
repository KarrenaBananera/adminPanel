using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using tableRazorAssigment.Data;

namespace tableRazorAssigment.Pages;

public class EmailConfirmationModel : PageModel
{
    private readonly UserManager<User> _userManager;

    public EmailConfirmationModel(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> OnGetAsync(string? userId, string? code)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
        {
            return RedirectToPage("Index");
        }
        var identityResult = await ConfirmEmailAsync(userId, code);
        if (identityResult is null || identityResult.Succeeded == false)
        {
            ViewData["Error"] = "Something went wrong";
        }
        return Page();
    }

    private async Task<IdentityResult?> ConfirmEmailAsync(string userId, string code)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return null;
        var identityResult = await _userManager.ConfirmEmailAsync(user, code);
        await OnSuccsessConfirm(user, identityResult);
        return identityResult;
    }

    private async Task OnSuccsessConfirm(User user, IdentityResult? result)
    {
        if (result is not null && result.Succeeded == true)
        {
            user.IsUserEmailConfirmed = true;
            await _userManager.UpdateAsync(user);
        }
    }

}