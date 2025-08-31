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
        {
            return pagingParametersResult.Error;
        }

        PagingParameters pagingParameters = pagingParametersResult.Value;

        // Parse sorting parameters
        Result<(UsersSortBy sortBy, bool ascending)> sortResult = ParseSortParameter(query.Sort);
        if (sortResult.IsFailure)
        {
            return sortResult.Error;
        }

        (UsersSortBy sortBy, bool ascending) = sortResult.Value;

        // Execute query directly with LINQ
        IQueryable<User> queryable = queryContext.Users.AsNoTracking();

        // Apply filtering using implicit string conversion
        if (!string.IsNullOrWhiteSpace(query.Email))
        {
            queryable = queryable.Where(u => ((string)u.Email).Contains(query.Email));
        }

        if (!string.IsNullOrWhiteSpace(query.UserName))
        {
            queryable = queryable.Where(u => ((string)u.UserName).Contains(query.UserName));
        }

        // Apply sorting
        queryable = sortBy switch
        {
            UsersSortBy.UserName => ascending
                ? queryable.OrderBy(u => u.UserName)
                : queryable.OrderByDescending(u => u.UserName),
            UsersSortBy.Email => ascending
                ? queryable.OrderBy(u => u.Email)
                : queryable.OrderByDescending(u => u.Email),
            UsersSortBy.LastLoginAt => ascending
                ? queryable.OrderBy(u => u.LastLoginAt)
                : queryable.OrderByDescending(u => u.LastLoginAt),
            _ => ascending
                ? queryable.OrderBy(u => u.CreatedAt)
                : queryable.OrderByDescending(u => u.CreatedAt)
        };

        // Get total count before pagination
        int totalItems = await queryable.CountAsync();
        int totalPages = (int)Math.Ceiling((double)totalItems / pagingParameters.Limit);

        // Apply pagination and project to DTOs
        List<UserDto> userDtoList = await queryable
            .Skip((pagingParameters.Page - 1) * pagingParameters.Limit)
            .Take(pagingParameters.Limit)
            .Select(u => new UserDto(
                u.Id.Value,
                u.Email.Value.Address,
                u.UserName.Value,
                u.CreatedAt,
                u.LastLoginAt
            ))
            .ToListAsync();

        System.Collections.ObjectModel.ReadOnlyCollection<UserDto> userDtos = userDtoList.AsReadOnly();

        var paginationInfo = new PaginationInfo(
            totalItems,
            totalPages,
            pagingParameters.Page,
            pagingParameters.Limit
        );

        return new GetUsersResult(userDtos, paginationInfo);
    }

    private static Result<(UsersSortBy sortBy, bool ascending)> ParseSortParameter(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return (UsersSortBy.CreatedAt, true);
        }

        bool ascending = true;
        string fieldName = sort;

        if (sort.StartsWith('-'))
        {
            ascending = false;
            fieldName = sort[1..];
        }

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
}
