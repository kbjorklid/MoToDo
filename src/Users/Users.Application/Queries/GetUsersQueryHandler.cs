using Base.Domain;
using Base.Domain.Result;
using Microsoft.EntityFrameworkCore;
using Users.Application.Ports;
using Users.Contracts;
using Users.Domain;

namespace Users.Application.Queries;

/// <summary>
/// Handles the GetUsersQuery to retrieve users with pagination and sorting.
/// </summary>
public static class GetUsersQueryHandler
{
    /// <summary>
    /// Handles the query to retrieve users using Wolverine's preferred static method pattern with CQRS principles - direct database querying via query context.
    /// </summary>
    /// <param name="query">The get users query containing pagination and sorting parameters.</param>
    /// <param name="queryContext">The query context for direct database access.</param>
    /// <returns>A Result containing the GetUsersResult if successful, or an error if validation fails.</returns>
    public static async Task<Result<GetUsersResult>> Handle(GetUsersQuery query, IUsersQueryContext queryContext)
    {
        Result<PagingParameters> pagingParametersResult = PagingParameters.Create(
            query.Page ?? PagingParameters.DefaultPage,
            query.Limit ?? PagingParameters.DefaultLimit
        );

        if (pagingParametersResult.IsFailure)
            return pagingParametersResult.Error;

        PagingParameters pagingParameters = pagingParametersResult.Value;

        IQueryable<User> queryable = queryContext.Users.AsNoTracking();
        queryable = ApplyFiltering(queryable, query);
        queryable = ApplySorting(queryable, query.Sort);

        int totalItems = await queryable.CountAsync();
        int totalPages = (int)Math.Ceiling((double)totalItems / pagingParameters.Limit);

        List<UserDto> userDtos = await queryable
            .Skip((pagingParameters.Page - 1) * pagingParameters.Limit)
            .Take(pagingParameters.Limit)
            .Select(MapToUserDto)
            .ToListAsync();

        var paginationInfo = new PaginationInfo(totalItems, totalPages, pagingParameters.Page, pagingParameters.Limit);
        return new GetUsersResult(userDtos.AsReadOnly(), paginationInfo);
    }

    private static (UsersSortBy sortBy, bool ascending) ParseSortParameter(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
            return (UsersSortBy.CreatedAt, true);

        bool ascending = !sort.StartsWith('-');
        string fieldName = ascending ? sort : sort[1..];

        UsersSortBy sortBy = fieldName.ToLowerInvariant() switch
        {
            "username" => UsersSortBy.UserName,
            "email" => UsersSortBy.Email,
            "createdat" => UsersSortBy.CreatedAt,
            "lastloginat" => UsersSortBy.LastLoginAt,
            _ => UsersSortBy.CreatedAt
        };

        return (sortBy, ascending);
    }

    private static IQueryable<User> ApplyFiltering(IQueryable<User> queryable, GetUsersQuery query)
    {
        if (!string.IsNullOrWhiteSpace(query.Email))
            queryable = queryable.Where(u => ((string)u.Email).Contains(query.Email));

        if (!string.IsNullOrWhiteSpace(query.UserName))
            queryable = queryable.Where(u => ((string)u.UserName).Contains(query.UserName));

        return queryable;
    }

    private static IQueryable<User> ApplySorting(IQueryable<User> queryable, string? sort)
    {
        (UsersSortBy sortBy, bool ascending) = ParseSortParameter(sort);

        return sortBy switch
        {
            UsersSortBy.UserName => ascending ? queryable.OrderBy(u => u.UserName) : queryable.OrderByDescending(u => u.UserName),
            UsersSortBy.Email => ascending ? queryable.OrderBy(u => u.Email) : queryable.OrderByDescending(u => u.Email),
            UsersSortBy.LastLoginAt => ascending ? queryable.OrderBy(u => u.LastLoginAt) : queryable.OrderByDescending(u => u.LastLoginAt),
            _ => ascending ? queryable.OrderBy(u => u.CreatedAt) : queryable.OrderByDescending(u => u.CreatedAt)
        };
    }

    private static System.Linq.Expressions.Expression<Func<User, UserDto>> MapToUserDto =>
        u => new UserDto(u.Id.Value, u.Email.Value.Address, u.UserName.Value, u.CreatedAt, u.LastLoginAt);
}
