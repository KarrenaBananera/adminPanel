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
    private readonly IPasswordRecoveryService passwordRecovery;

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

    public ResetPasswordModel(IPasswordRecoveryService passwordRecovery)
    {
        this.passwordRecovery = passwordRecovery;
    }

    public async Task<IActionResult> OnGet(string? userId, string? code)
    {
        if (await IsRequestValid(userId, code) == false)
        {
            ViewData["Error"] = "Something went wrong";
            return Page();
        }
        Input = new InputModel
        {
            UserId = userId,
            Code = code
        };
        return Page();
    }

    private async Task<bool> IsRequestValid(string? userId, string? code)
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
        var result = await passwordRecovery.RecoverAccountAsync(Input.UserId, Input.Code, Input.NewPassword);
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

}
