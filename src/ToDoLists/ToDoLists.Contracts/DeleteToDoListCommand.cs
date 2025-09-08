namespace ToDoLists.Contracts;

/// <summary>
/// Command to delete an existing todo list.
/// </summary>
public sealed record DeleteToDoListCommand
{
    /// <summary>
    /// The unique identifier of the todo list to delete.
    /// </summary>
    public required string ToDoListId { get; init; }

    /// <summary>
    /// The unique identifier of the user (for authorization).
    /// </summary>
    public required string UserId { get; init; }
}

/// <summary>
/// Result of successfully deleting a todo list.
/// </summary>
public sealed record DeleteToDoListResult
{
    /// <summary>
    /// The unique identifier of the deleted todo list.
    /// </summary>
    public Guid ToDoListId { get; init; }

    /// <summary>
    /// The unique identifier of the user who owned the list.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// The date and time when the todo list was deleted.
    /// </summary>
    public DateTime DeletedAt { get; init; }
}
