using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using tableRazorAssigment.Data;

namespace tableRazorAssigment.Services;

public class CustomSignInManager : SignInManager<User>
{
    public CustomSignInManager(UserManager<User> userManager,
        IHttpContextAccessor contextAccessor,
        IUserClaimsPrincipalFactory<User> claimsFactory,
        IOptions<IdentityOptions> optionsAccessor,
        ILogger<SignInManager<User>> logger,
        IAuthenticationSchemeProvider schemes,
        IUserConfirmation<User> confirmation)
        : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
    }

    public async Task<CustomSignInResult> CustomPasswordSignInAsync(string email, string password, bool isPersistent, bool lockoutOnFailure)
    {
        var user = await UserManager.FindByEmailAsync(email);
        if (user == null)
            return new CustomSignInResult { Succeeded = false};
        if (user.Status == UserStatus.Blocked)
            return new CustomSignInResult { Succeeded = false, IsBlocked = true };
        var result = await PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
        return new CustomSignInResult
        {
            Succeeded = result.Succeeded,
            IsLockedOut = result.IsLockedOut,
            IsNotAllowed = result.IsNotAllowed,
            RequiresTwoFactor = result.RequiresTwoFactor,
            ErrorMessage = result.Succeeded ? null : "InvalidPassword"
        };
    }
 }