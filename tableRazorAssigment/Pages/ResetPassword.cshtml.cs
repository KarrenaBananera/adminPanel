using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using tableRazorAssigment.Data;
using tableRazorAssigment.Pages.Shared;
using tableRazorAssigment.Services;

namespace tableRazorAssigment.Pages;

public class ResetPasswordModel : GuestOnlyPage
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
        [Compare("Password", ErrorMessage = "Passwords doesn't match")]
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
        if (User.Identity is not null && User.Identity.IsAuthenticated)
            return RedirectToPage("Index");
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
            return RedirectToPage("Index");

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return RedirectToPage("Index");

        var isValid = await _userManager.VerifyUserTokenAsync(
              user,
              _userManager.Options.Tokens.PasswordResetTokenProvider,
              "ResetPassword",
              code
          );
        if (!isValid)
            return RedirectToPage("Index");

        Input = new InputModel
        {
            UserId = userId,
            Code = code
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)

            return Page();
        var user = await _userManager.FindByIdAsync(Input.UserId);
        if (user is null)
            return RedirectToPage("Index");

        var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.NewPassword);

        if (result.Succeeded)
        {
            await _signInManager.CustomPasswordSignInAsync(user.Email, Input.NewPassword, true, false);
            return RedirectToPage("Index");

        }
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
        return Page();
    }
}
