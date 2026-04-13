using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using tableRazorAssigment.Data;
using tableRazorAssigment.Pages.Shared;
using tableRazorAssigment.Services;

namespace tableRazorAssigment.Pages;

public class RegisterModel : GuestOnlyPage
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IEmailService emailSender;

    public RegisterModel(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IEmailService emailSender)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        this.emailSender = emailSender;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Specify User name")]
        [Display(Name = "Name")]
        [MaxLength(256)]
        public string UserName { get; set; }

        [Display(Name = "Your title")]
        [MaxLength(200)]
        public string? UserTitle { get; set; }


        [Required(ErrorMessage = "Email required")]
        [EmailAddress(ErrorMessage = "Wrong email format")]
        [Display(Name = "Email")]
        [MaxLength(256)]

        public string Email { get; set; }

        [Required(ErrorMessage = "Password required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [MaxLength(256)]

        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "Passwords doesn't match")]
        [MaxLength(256)]
        public string ConfirmPassword { get; set; }
    }

    private User ConstructUser()
    {
        return new User
        {
            UserName = Input.Email,
            Name = Input.UserName,
            Email = Input.Email,
            Title = Input.UserTitle,
            UserEmail = Input.Email
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();
        var result = await CreateUser();
        if (CheckInvalidCreate(result))
            return Page();
        return RedirectToPage("Index");
    }

    private async Task<IdentityResult> CreateUser()
    {
        var user = ConstructUser();
        await CheckExistingUser(user);
        var result = await _userManager.CreateAsync(user, Input.Password);
        await OnUserCreation(result, user);
        return result;
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
    private bool CheckInvalidCreate(IdentityResult result)
    {
        if (result is null || result.Succeeded == false)
        {
            OnInvalidCreate(result);
            return true;
        }
        return false;
    }

    private void OnInvalidCreate(IdentityResult result)
    {
        ModelState.AddModelError(Input.Email, "This email already exist");
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
        await emailSender.SendEmailAsync(user.Email, "The app email confirmation", recoveryMessage);
    }

    private async Task<string> GenerateConfirmationLink(User user)
    {
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var callbackUrl = Url.Page("EmailConfirmation", pageHandler: null,
           new { userId = user.Id, code = code }, protocol: Request.Scheme);
        return callbackUrl;
    }

}
