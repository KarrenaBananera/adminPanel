using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using tableRazorAssigment.Data;
using tableRazorAssigment.Model;
using tableRazorAssigment.Services;

namespace tableRazorAssigment.Pages;

public class IndexModel : PageModel
{
    public IndexModel(UserManager<User> userManager)
    {
    }

    public async Task<IActionResult> OnGet()
    {
        return Page();
    }

}
