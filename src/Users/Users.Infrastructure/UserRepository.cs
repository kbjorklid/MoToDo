using Base.Domain;
using Base.Domain.Result;
using Microsoft.EntityFrameworkCore;
using Users.Domain;

namespace Users.Infrastructure;

/// <summary>
/// EF Core implementation of the User repository for persistence and retrieval operations.
/// </summary>
internal sealed class UserRepository : IUserRepository
{
    private readonly UsersDbContext _context;

    public UserRepository(UsersDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<User?> GetByIdAsync(UserId userId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetByEmailAsync(Email email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByUserNameAsync(UserName userName)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.UserName == userName);
    }

    public async Task AddAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        await _context.Users.AddAsync(user);
    }

    public async Task<PagedResult<User>> GetAllAsync(PagingParameters pagingParameters)
    {
        // Get total count
        int totalItems = await _context.Users.CountAsync();

        // Early return if no results
        if (totalItems == 0)
        {
            return PagedResult<User>.Empty(pagingParameters);
        }

        // Apply pagination with default sorting by CreatedAt
        List<User> users = await _context.Users
            .OrderBy(u => u.CreatedAt)
            .Skip(pagingParameters.Skip)
            .Take(pagingParameters.Limit)
            .ToListAsync();

        return PagedResult<User>.Create(users, totalItems, pagingParameters);
    }

    public async Task<PagedResult<User>> FindUsersAsync(UserQueryCriteria criteria, CancellationToken cancellationToken = default)
    {
        IQueryable<User> query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(criteria.EmailFilter))
        {
            query = query.Where(u => EF.Functions.ILike(u.Email, $"%{criteria.EmailFilter}%"));
        }

        if (!string.IsNullOrWhiteSpace(criteria.UserNameFilter))
        {
            query = query.Where(u => EF.Functions.ILike(u.UserName, $"%{criteria.UserNameFilter}%"));
        }

        // Get total count before applying pagination
        int totalItems = await query.CountAsync(cancellationToken);

        // Early return if no results
        if (totalItems == 0)
        {
            return PagedResult<User>.Empty(criteria.PagingParameters);
        }

        // Apply sorting using EF.Property to access the database columns directly
        query = criteria.SortBy switch
        {
            UsersSortBy.UserName => criteria.Ascending
                ? query.OrderBy(u => EF.Property<string>(u, "UserName"))
                : query.OrderByDescending(u => EF.Property<string>(u, "UserName")),
            UsersSortBy.Email => criteria.Ascending
                ? query.OrderBy(u => EF.Property<string>(u, "Email"))
                : query.OrderByDescending(u => EF.Property<string>(u, "Email")),
            UsersSortBy.LastLoginAt => criteria.Ascending
                ? query.OrderBy(u => u.LastLoginAt)
                : query.OrderByDescending(u => u.LastLoginAt),
            _ => criteria.Ascending
                ? query.OrderBy(u => u.CreatedAt)
                : query.OrderByDescending(u => u.CreatedAt)
        };

        // Apply pagination
        List<User> users = await query
            .Skip(criteria.PagingParameters.Skip)
            .Take(criteria.PagingParameters.Limit)
            .ToListAsync(cancellationToken);

        return PagedResult<User>.Create(users, totalItems, criteria.PagingParameters);
    }

    public Task UpdateAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        _context.Users.Update(user);
        return Task.CompletedTask;
    }

    public async Task<Result> DeleteAsync(UserId userId)
    {
        User? user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
        {
            return new Error(
                User.Codes.NotFound,
                $"User with ID '{userId}' was not found.",
                ErrorType.NotFound);
        }

        _context.Users.Remove(user);
        return Result.Success();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
