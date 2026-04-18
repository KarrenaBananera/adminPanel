namespace tableRazorAssigment.Services.Implenetation;

public class CustomSignInResult
{
    public bool Succeeded { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsLockedOut { get; set; }
    public bool IsNotAllowed { get; set; }
    public bool RequiresTwoFactor { get; set; }
    public string? ErrorMessage { get; set; }
}