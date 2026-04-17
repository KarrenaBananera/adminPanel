using Microsoft.AspNetCore.Identity;
using tableRazorAssigment.Data;

namespace tableRazorAssigment.Services;

public interface IUserRegisterService
{
    Task<IdentityResult?> RegisterUserAsync(User user, string password);

}
