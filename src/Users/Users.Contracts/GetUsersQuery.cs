namespace Users.Contracts;

/// <summary>
/// Query to retrieve a paginated list of users with optional filtering and sorting.
/// </summary>
public sealed record GetUsersQuery
{
    /// <summary>
    /// The page number to retrieve (1-based).
    /// </summary>
    public int? Page { get; init; }

    /// <summary>
    /// The number of items per page.
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    /// Sort field and direction (format: field or -field for descending).
    /// </summary>
    public string? Sort { get; init; }

    /// <summary>
    /// Filter users by email address (partial match).
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Filter users by username (partial match).
    /// </summary>
    public string? UserName { get; init; }
}
