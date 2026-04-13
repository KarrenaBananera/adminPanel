using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using tableRazorAssigment.Data;
using tableRazorAssigment.Pages.Shared;
using tableRazorAssigment.Services;

namespace tableRazorAssigment.Pages;

public class ForgotPasswordModel : GuestOnlyPage
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailService emailSender;

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; }
    }

    public ForgotPasswordModel(UserManager<User> userManager, IEmailService emailSender)
    {
        _userManager = userManager;
        this.emailSender = emailSender;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();
        ViewData["SuccessMessage"] = $"Recovery link was send on your email.";
        var user = await GetValidUserAsync();
        if (user is not null)
            await SendRecoveryEmail(user);
        return Page();
    }

    private async Task<User?> GetValidUserAsync()
    {
        var user = await _userManager.FindByEmailAsync(Input.Email);
        if (user == null || user.IsUserEmailConfirmed == false)
            return null;
        return user;
    }
    private async Task SendRecoveryEmail(User user)
    {
        if (user.IsUserBlocked == true)
        {
            BlockedUserView();
            return;
        }
        var recoveryUrl = await GenerateRecoveryUrlAsync(user);
        string recoveryMessage = $"Please click the following link to recover your account: {recoveryUrl}";
        await emailSender.SendEmailAsync(user.Email, "The app Account recovery", recoveryMessage);
    }

    private async Task<string> GenerateRecoveryUrlAsync(User user)
    {
        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        var callbackUrl = Url.Page("ResetPassword", pageHandler: null,
           new { userId = user.Id, code = code }, protocol: Request.Scheme);
        return callbackUrl;
    }
    private void BlockedUserView()
    {
        ViewData["ErrorMessage"] = $"Your account is blocked";
        ViewData["SuccessMessage"] = null;
    }
}