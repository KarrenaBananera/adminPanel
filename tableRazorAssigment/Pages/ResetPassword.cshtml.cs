using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using tableRazorAssigment.Data;
using tableRazorAssigment.Pages.Shared;
using tableRazorAssigment.Services;

namespace tableRazorAssigment.Pages;

public class ResetPasswordModel : PageModel
{
    private readonly CustomSignInManager _signInManager;
    private readonly UserManager<User> _userManager;

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string Code { get; set; }

        [Required(ErrorMessage = "Password required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [MaxLength(256)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("NewPassword", ErrorMessage = "Passwords doesn't match")]
        [MaxLength(256)]
        public string ConfirmPassword { get; set; }
    }

    public ResetPasswordModel(CustomSignInManager signInManager, UserManager<User> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public async Task<IActionResult> OnGet(string? userId, string? code)
    {
       if (IsRequestValid(userId, code) == false || IsRequestValid(userId, code) == false)
            return RedirectToPage("Index");
        Input = new InputModel
        {
            UserId = userId,
            Code = code
        };
        return Page();
    }

    private async Task<bool> IsUserTokenValid(string userId, string code)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return false;
        var isValid = await _userManager.VerifyUserTokenAsync(
             user,
             _userManager.Options.Tokens.PasswordResetTokenProvider,
             "ResetPassword",
             code
         );
        return isValid;
    }

    private bool IsRequestValid(string? userId, string? code)
    {
        if (User.Identity is not null && User.Identity.IsAuthenticated)
            return false;
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
            return false;
        return true;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();
        var result = await ResetUserPassword();
        if (result is not null && result.Succeeded)
            return RedirectToPage("Index");
        AddErrorsToView(result);
        return Page();
    }

    private void AddErrorsToView(IdentityResult? result)
    {
        if (result is null)
            return;
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    private async Task<IdentityResult?> ResetUserPassword()
    {
        var user = await _userManager.FindByIdAsync(Input.UserId);
        if (user is null)
            return null;
        var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.NewPassword);
        await OnSuccessResult(result, user);
        return result;
    }

    private async Task OnSuccessResult(IdentityResult? result, User user)
    {
        if (result is not null && result.Succeeded)
        {
            await _signInManager.CustomPasswordSignInAsync(user.Email, Input.NewPassword, true, false);
        }
    }

}
