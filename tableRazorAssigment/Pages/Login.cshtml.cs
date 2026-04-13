using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using tableRazorAssigment.Data;
using tableRazorAssigment.Pages.Shared;
using tableRazorAssigment.Services;

namespace tableRazorAssigment.Pages;

public class LoginModel : GuestOnlyPage
{
    private readonly CustomSignInManager _signInManager;
    private readonly UserManager<User> _userManager;

    public LoginModel(CustomSignInManager signInManager,
                      UserManager<User> userManager
                      )
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public string ReturnUrl { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Email required")]
        [EmailAddress(ErrorMessage = "Wrong email format")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();
        var result = await _signInManager.CustomPasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
        if (result is not null && result.Succeeded)
            return RedirectToPage("Index");
        AddModelErrors(result);
        return Page();
    }

    private void AddModelErrors(CustomSignInResult? result)
    {
        if (result is null)
            return;
        if (result.IsBlocked)
            ModelState.AddModelError(string.Empty, "User blocked");
        else if (result.IsLockedOut)
            ModelState.AddModelError(string.Empty, "Too many login attempts");
        else
            ModelState.AddModelError(string.Empty, "Incorrect email or password");
    }

}