using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System.ComponentModel.DataAnnotations;
using tableRazorAssigment.Data;
using tableRazorAssigment.Pages.Shared;
using tableRazorAssigment.Services;

namespace tableRazorAssigment.Pages;

public class RegisterModel : GuestOnlyPage
{
    private readonly IUserRegisterService _registerService;

    public RegisterModel(IUserRegisterService registerService)
    {
        _registerService = registerService;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Specify User name")]
        [Display(Name = "Name")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-_\.!?@#\$%&*()+=:;',\""]*$",
         ErrorMessage = "Name contains invalid characters.")]
        [MaxLength(256)]
        public string UserName { get; set; }

        [Display(Name = "Your title")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-_\.!?@#\$%&*()+=:;',\""]*$",
        ErrorMessage = "Title contains invalid characters.")]
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
        var user = ConstructUser();
        var result = await _registerService.RegisterUserAsync(user, Input.Password);
        if (CheckInvalidCreate(result))
            return Page();
        return RedirectToPage("Index");
    }

    private bool CheckInvalidCreate(IdentityResult? result)
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

}
