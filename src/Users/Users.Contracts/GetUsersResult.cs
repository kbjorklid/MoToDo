using Base.Contracts;

namespace Users.Contracts;

/// <summary>
/// Result of successfully retrieving users with pagination information.
/// </summary>
/// <param name="Data">The list of user DTOs returned by the query.</param>
/// <param name="Pagination">Pagination information including totals and current page.</param>
public sealed record GetUsersResult(
    IReadOnlyList<UserDto> Data,
    PaginationInfo Pagination
);
