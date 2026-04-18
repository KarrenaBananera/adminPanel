using tableRazorAssigment.Data;
using tableRazorAssigment.Model;

namespace tableRazorAssigment.Services;

public interface IUserFetcher
{
    Task<List<UserDto>> FetchUsersAsync(UserQueryParameters parameters);

    Task<int> TotalCountAsync(UserQueryParameters parameters);

    Task<UserDto> GetCurrentUserAsync(HttpContext context);

}
