using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using tableRazorAssigment.Data;
using tableRazorAssigment.Pages.Shared;
using tableRazorAssigment.Services;

namespace tableRazorAssigment.Pages;

public class ForgotPasswordModel : GuestOnlyPage {
    private readonly IPasswordRecoveryService _recoveryService;

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; }
    }

    public ForgotPasswordModel(IPasswordRecoveryService recoveryService)
    {
        _recoveryService = recoveryService;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();
        ViewData["SuccessMessage"] = $"Recovery link was send on your email.";
        var result = await _recoveryService.SendRecoveryEmailAsync(Input.Email);
        if (result == RecoveryResult.UserBlocked)
            BlockedUserView();
        return Page();
    }

    private void BlockedUserView()
    {
        ViewData["ErrorMessage"] = $"Your account is blocked";
        ViewData["SuccessMessage"] = null;
    }
}