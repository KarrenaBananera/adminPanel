using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using tableRazorAssigment.Data;
using tableRazorAssigment.Pages.Shared;

namespace tableRazorAssigment.Pages;

public class RegisterModel : GuestOnlyPage
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public RegisterModel(
        UserManager<User> userManager,
        SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
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
            UserName = Input.UserName,
            Email = Input.Email,
            Title = Input.UserTitle,
            UserEmail = Input.Email,
            Status = UserStatus.Unverified
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();
        var user = ConstructUser();
        var result = CreateUser(user);
        if (result is null)
        {

        }
        OnUserCreation(result);

        return Page();
    }

    private async Task<IdentityResult?> CreateUser(User user)
    {
        try
        {
            return await _userManager.CreateAsync(user, Input.Password);
        }
        catch (DbUpdateException)
        {
            return null;
        }
    }

    private async Task OnUserCreation(IdentityResult result, User user)
    {
        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: true);
        }
    }

}
