using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using tableRazorAssigment.Data;
using tableRazorAssigment.Pages.Shared;
using tableRazorAssigment.Services;

namespace tableRazorAssigment.Services;

public class PasswordRecoveryService : IPasswordRecoveryService
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailSender;
    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CustomSignInManager _signInManager;

    public PasswordRecoveryService(IEmailService emailSender, UserManager<User> userManager, LinkGenerator linkGenerator,
                           IHttpContextAccessor httpContextAccessor, CustomSignInManager signInManager)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _linkGenerator = linkGenerator;
        _httpContextAccessor = httpContextAccessor;
        _signInManager = signInManager;
    }

    public async Task<RecoveryResult> SendRecoveryEmailAsync(string email)
    {
        var user = await GetValidUserAsync(email);
        if (user is null)
            return RecoveryResult.Failed;
        if (user.IsUserBlocked)
            return RecoveryResult.UserBlocked;
        var recoveryUrl = await GenerateRecoveryUrlAsync(user);
        string recoveryMessage = $"Please click the following link to recover your account: {recoveryUrl}";
        await _emailSender.SendEmailAsync(user.Email, "The app Account recovery", recoveryMessage);
        return RecoveryResult.Success;
    }

    private async Task<User?> GetValidUserAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null 
        || user.IsUserEmailConfirmed == false)
            return null;
        return user;
    }

    public async Task<string> GenerateRecoveryUrlAsync(User user)
    {
        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No HTTP context available.");
        var callbackUrl = _linkGenerator.GetUriByPage(
            httpContext,
            page: "/ResetPassword",       
            handler: null,
            values: new { userId = user.Id, code = code },
            scheme: httpContext.Request.Scheme,
            host: httpContext.Request.Host);
        return callbackUrl;
    }

    public async Task<IdentityResult?> RecoverAccountAsync(string userId, string code, string password)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return null;
        if (await IsUserTokenValidAsync(user, code))
            return await ResetUserPasswordAsync(user, code, password);
        return null;
    }

    private async Task<IdentityResult?> ResetUserPasswordAsync(User user, string code, string password)
    {
        var result = await _userManager.ResetPasswordAsync(user, code, password);
        if (result is not null && result.Succeeded)
        {
            await OnSuccessResult(result, user, password);
        }
        return result;
    }

    private async Task OnSuccessResult(IdentityResult? result, User user, string password)
    {
        await _signInManager.CustomSignInAsync(user.Email, password, true, false);
    }

    private async Task<bool> IsUserTokenValidAsync(User user, string code)
    {
        var isValid = await _userManager.VerifyUserTokenAsync(
             user,
             _userManager.Options.Tokens.PasswordResetTokenProvider,
             "ResetPassword",
             code
         );
        return isValid;
    }

}
