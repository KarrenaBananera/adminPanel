using Microsoft.AspNetCore.Identity;
using tableRazorAssigment.Data;

namespace tableRazorAssigment.Services;

public interface IPasswordRecoveryService
{
    Task<RecoveryResult> SendRecoveryEmailAsync(string email);
    Task<IdentityResult?> RecoverAccountAsync(string userId, string code, string NewPassword);

}
public enum RecoveryResult
{
    UserBlocked, Failed, Success
}