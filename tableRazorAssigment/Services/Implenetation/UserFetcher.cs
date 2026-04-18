using Microsoft.EntityFrameworkCore;
using tableRazorAssigment.Data;
using tableRazorAssigment.Model;

namespace tableRazorAssigment.Services.Implenetation;

public class UserFetcher : IUserFetcher
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UserFetcher(ApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<UserDto?> GetCurrentUserAsync(HttpContext context)
    {
        var user = await _currentUserService.GetCurrentUserAsync(context);
        if (user is null)
            return null;
        return ToUserDto(user);
    }

    public async Task<List<UserDto>> FetchUsersAsync(UserQueryParameters parameters)
    {
        IQueryable<User> usersQuery = _dbContext.Users;
        var filteredUsers = FilterQuery(usersQuery,parameters);
        var paginated = Pagination(filteredUsers, parameters);
        var resultQuery = ToUserDto(paginated, parameters);
        return await resultQuery.ToListAsync();
    }

    public async Task<int> TotalCountAsync(UserQueryParameters parameters)
    {
        IQueryable<User> users = _dbContext.Users;
        var filteredUsers = FilterQuery(users, parameters);
        return await filteredUsers.CountAsync();
    }

    private IQueryable<User> FilterQuery(IQueryable<User> query, UserQueryParameters parameters)
    {
        query = SearchFilter(query,parameters);
        query = Sorting(query, parameters);
        return query;
    }

    private IQueryable<User> SearchFilter(IQueryable<User> query, UserQueryParameters parameters)
    {
        if (parameters.SearchTerm is null)
            return query;
        var searchTermLower = parameters.SearchTerm.Trim().ToLower();
        query = query.Where(u =>
            u.Name != null && u.Name.ToLower().Contains(searchTermLower) ||
            u.UserEmail != null && u.UserEmail.ToLower().Contains(searchTermLower) ||
            u.Title != null && u.Title.ToLower().Contains(searchTermLower)
        );
        return query;
    }

    private IQueryable<User> Sorting(IQueryable<User> query, UserQueryParameters parameters)
    {
        if (parameters.SortItemsBy is null)
            return query.OrderBy(u => u.Email);
        bool descending = parameters.SortDescending;
        query = parameters.SortItemsBy switch
        {
            UserQueryParameters.SortBy.Email => SortByEmail(query, descending),
            UserQueryParameters.SortBy.Name => SortByName(query, descending),
            UserQueryParameters.SortBy.Status => SortByStatus(query, descending),
            UserQueryParameters.SortBy.LastSeen => SortByLastSeen(query, descending),
            _ => SortByEmail(query, descending)
        };
        return query;
    }

    private IQueryable<User> SortByStatus(IQueryable<User> query, bool descending)
    {
        if (descending)
            query = query.OrderByDescending(u => u.IsUserBlocked)
                         .ThenByDescending(u => u.IsUserEmailConfirmed);
        else
            query = query.OrderBy(u => u.IsUserBlocked)
                         .ThenBy(u => u.IsUserEmailConfirmed);
        return query;
    }
    private IQueryable<User> SortByLastSeen(IQueryable<User> query, bool descending)
    {
        return descending ? query.OrderByDescending(u => u.LastSeen) : query.OrderBy(u => u.LastSeen);
    }

    private IQueryable<User> SortByEmail(IQueryable<User> query, bool descending)
    {
        return descending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email);
    }

    private IQueryable<User> SortByName(IQueryable<User> query, bool descending)
    {
        return descending ? query.OrderByDescending(u => u.Name) : query.OrderBy(u => u.Name);
    }

    private IQueryable<User> Pagination(IQueryable<User> query, UserQueryParameters parameters)
    {
        return query
        .Skip((parameters.PageNumber - 1) * parameters.PageSize)
        .Take(parameters.PageSize);
    }

    private IQueryable<UserDto> ToUserDto(IQueryable<User> query, UserQueryParameters parameters)
    {
        return query.Select(u => ToUserDto(u));
    }

    private static UserDto ToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Title = user.Title,
            Name = user.Name,
            UserEmail = user.UserEmail,
            IsUserBlocked = user.IsUserBlocked,
            IsUserEmailConfirmed = user.IsUserEmailConfirmed,
            LastSeen = user.LastSeen
        };
    }

}
