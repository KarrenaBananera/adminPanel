using tableRazorAssigment.Data;

namespace tableRazorAssigment.Services;

public interface ICurrentUserService
{
    Task<User?> GetCurrentUserAsync(HttpContext context);

}
