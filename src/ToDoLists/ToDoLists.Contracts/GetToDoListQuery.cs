namespace ToDoLists.Contracts;

/// <summary>
/// Query to retrieve a specific todo list by its identifier.
/// </summary>
public sealed record GetToDoListQuery
{
    /// <summary>
    /// The unique identifier of the todo list to retrieve.
    /// </summary>
    public required string ToDoListId { get; init; }

    /// <summary>
    /// The unique identifier of the user making the request (for authorization).
    /// </summary>
    public required string UserId { get; init; }
}
