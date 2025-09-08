namespace ToDoLists.Contracts;

/// <summary>
/// Query to retrieve a paginated list of todo lists with optional sorting.
/// </summary>
public sealed record GetToDoListsQuery
{
    /// <summary>
    /// The unique identifier of the user whose todo lists to retrieve.
    /// </summary>
    public required string UserId { get; init; }

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
}
