namespace tableRazorAssigment.Services;

public interface IUserManagerService
{
    Task DeleteUsersAsync(List<string> userIds);

    Task BLockUsersAsync(List<string> userIds);

    Task UnblockUsersAsync(List<string> userIds);

    Task DeleteUnverifiedAsync(List<string> userIds);

}
