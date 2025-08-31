namespace Users.Contracts;

/// <summary>
/// Query to retrieve a paginated list of users with optional filtering and sorting.
/// </summary>
/// <param name="Page">The page number to retrieve (1-based).</param>
/// <param name="Limit">The number of items per page.</param>
/// <param name="Sort">Sort field and direction (format: field or -field for descending).</param>
/// <param name="Email">Filter users by email address (partial match).</param>
/// <param name="UserName">Filter users by username (partial match).</param>
public sealed record GetUsersQuery(
    int? Page = null,
    int? Limit = null,
    string? Sort = null,
    string? Email = null,
    string? UserName = null
);
