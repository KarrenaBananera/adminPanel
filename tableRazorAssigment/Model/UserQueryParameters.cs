namespace tableRazorAssigment.Model;

public class UserQueryParameters
{
    private const int MaxPageSize = 50;
    private int _pageSize = 20;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }

    public string? SearchTerm { get; set; }

    public SortBy? SortItemsBy { get; set; }

    public bool SortDescending { get; set; }

    public enum SortBy
    {
        Email, Name, Status, LastSeen
    }

}
