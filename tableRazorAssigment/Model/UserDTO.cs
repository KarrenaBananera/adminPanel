namespace tableRazorAssigment.Model;

public class UserDto
{
    public string Id { get; set; }

    public string? Title { get; set; }

    public string Name { get; set; }

    public string UserEmail { get; set; }

    public bool IsUserBlocked { get; set; }

    public bool IsUserEmailConfirmed { get; set; }

    public DateTime LastSeen { get; set; }

}