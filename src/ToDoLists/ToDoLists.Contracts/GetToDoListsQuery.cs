namespace ToDoLists.Contracts;

/// <summary>
/// Query to retrieve a paginated list of todo lists with optional sorting.
/// </summary>
/// <param name="UserId">The unique identifier of the user whose todo lists to retrieve.</param>
/// <param name="Page">The page number to retrieve (1-based).</param>
/// <param name="Limit">The number of items per page.</param>
/// <param name="Sort">Sort field and direction (format: field or -field for descending).</param>
public sealed record GetToDoListsQuery(
    string UserId,
    int? Page = null,
    int? Limit = null,
    string? Sort = null);
