using Base.Domain;
using Base.Domain.Result;
using Users.Contracts;
using Users.Domain;

namespace Users.Application.Queries;

/// <summary>
/// Handles the GetUsersQuery to retrieve users with pagination and sorting.
/// </summary>
public static class GetUsersQueryHandler
{
    /// <summary>
    /// Handles the query to retrieve users using Wolverine's preferred static method pattern.
    /// </summary>
    /// <param name="query">The get users query containing pagination and sorting parameters.</param>
    /// <param name="userRepository">The user repository injected by Wolverine.</param>
    /// <returns>A Result containing the GetUsersResult if successful, or an error if validation fails.</returns>
    public static async Task<Result<GetUsersResult>> Handle(GetUsersQuery query, IUserRepository userRepository)
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

        // Build query criteria
        UserQueryCriteria.UserQueryCriteriaBuilder criteriaBuilder = UserQueryCriteria.Builder(pagingParameters)
            .WithSortBy(sortBy, ascending);

        // Apply filtering if provided
        if (!string.IsNullOrWhiteSpace(query.Email))
        {
            criteriaBuilder = criteriaBuilder.WithEmailFilter(query.Email);
        }

        if (!string.IsNullOrWhiteSpace(query.UserName))
        {
            criteriaBuilder = criteriaBuilder.WithUserNameFilter(query.UserName);
        }

        Result<UserQueryCriteria> criteriaResult = criteriaBuilder.Build();

        if (criteriaResult.IsFailure)
        {
            return criteriaResult.Error;
        }

        UserQueryCriteria criteria = criteriaResult.Value;

        // Execute query
        PagedResult<User> pagedUsers = await userRepository.FindUsersAsync(criteria);

        // Map to DTOs
        System.Collections.ObjectModel.ReadOnlyCollection<UserDto> userDtos = pagedUsers.Data
            .Select(user => new UserDto(
                user.Id.Value,
                user.Email.Value.Address,
                user.UserName.Value,
                user.CreatedAt,
                user.LastLoginAt
            ))
            .ToList()
            .AsReadOnly();

        var paginationInfo = new PaginationInfo(
            pagedUsers.TotalItems,
            pagedUsers.TotalPages,
            pagedUsers.CurrentPage,
            pagedUsers.Limit
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
