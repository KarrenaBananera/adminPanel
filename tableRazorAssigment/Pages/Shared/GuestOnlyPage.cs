using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace tableRazorAssigment.Pages.Shared;

public abstract class GuestOnlyPage : PageModel
{
    public virtual async Task<IActionResult> OnGet()
    {
        if (User.Identity is not null && User.Identity.IsAuthenticated)
        {
            return RedirectToPage("Index");
        }
        return Page();
    }
}
