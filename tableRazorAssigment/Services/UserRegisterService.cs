using Azure.Core;
using Microsoft.AspNetCore.Identity;
using System;
using tableRazorAssigment.Data;
using tableRazorAssigment.Pages;

namespace tableRazorAssigment.Services;

public class UserRegisterService : IUserRegisterService
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailSender;
    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CustomSignInManager _signInManager;

    public UserRegisterService(IEmailService emailSender, UserManager<User> userManager, LinkGenerator linkGenerator,
                           IHttpContextAccessor httpContextAccessor, CustomSignInManager signInManager)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _linkGenerator = linkGenerator;
        _httpContextAccessor = httpContextAccessor;
        _signInManager = signInManager;
    }

    public async Task<IdentityResult?> RegisterUserAsync(User user, string password)
    {
        if (!CheckUserValid(user) || !CheckPasswordVallid(password))
            return null;
        return await CreateUserAsync(user, password);
    }

    private bool CheckUserValid(User user)
    {
        if (string.IsNullOrEmpty(user.Name) || string.IsNullOrEmpty(user.Email))
            return false;
        return true;
    }

    private bool CheckPasswordVallid(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;
        return true;
    }
    private async Task<IdentityResult?> CreateUserAsync(User user, string passowrd)
    {
        await CheckExistingUser(user);
        user.LastSeen = DateTime.UtcNow;
        var identityResult = await _userManager.CreateAsync(user, passowrd);
        await OnUserCreation(identityResult, user);
        return identityResult;
    }

    private async Task CheckExistingUser(User user)
    {
        var existenUser = await _userManager.FindByEmailAsync(user.Email);
        if (existenUser is not null)
            await DeleteUnverifiedUser(existenUser);
    }

    private async Task DeleteUnverifiedUser(User user)
    {
        if (user.IsUserEmailConfirmed == false)
        {
            await _userManager.DeleteAsync(user);
        }
    }

    private async Task OnUserCreation(IdentityResult result, User user)
    {
        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: true);
            await SendEmailConformation(user);
        }
    }

    private async Task SendEmailConformation(User user)
    {
        var callbackUrl = await GenerateConfirmationLink(user);
        string recoveryMessage = $"Please click the following link to confirm your email:\n{callbackUrl} \n\n Ignore this message if its not your account";
        await _emailSender.SendEmailAsync(user.Email, "The app email confirmation", recoveryMessage);
    }

    private async Task<string> GenerateConfirmationLink(User user)
    {
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var httpContext = _httpContextAccessor.HttpContext
           ?? throw new InvalidOperationException("No HTTP context available.");
        var callbackUrl = _linkGenerator.GetUriByPage(
            httpContext,
            page: "EmailConfirmation",
            handler: null,
            values: new { userId = user.Id, code = code },
            scheme: httpContext.Request.Scheme,
            host: httpContext.Request.Host);
        return callbackUrl;
    }

}
