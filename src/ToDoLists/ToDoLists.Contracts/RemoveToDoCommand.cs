namespace ToDoLists.Contracts;

/// <summary>
/// Command to remove a todo item from an existing todo list.
/// </summary>
public sealed record RemoveToDoCommand
{
    /// <summary>
    /// The unique identifier of the todo list.
    /// </summary>
    public required string ToDoListId { get; init; }

    /// <summary>
    /// The unique identifier of the todo item to remove.
    /// </summary>
    public required string ToDoId { get; init; }

    /// <summary>
    /// The unique identifier of the user (for authorization).
    /// </summary>
    public required string UserId { get; init; }
}

/// <summary>
/// Result of successfully removing a todo item from a list.
/// </summary>
public sealed record RemoveToDoResult
{
    /// <summary>
    /// The unique identifier of the todo list.
    /// </summary>
    public Guid ToDoListId { get; init; }

    /// <summary>
    /// The unique identifier of the removed todo item.
    /// </summary>
    public Guid ToDoId { get; init; }

    /// <summary>
    /// The unique identifier of the user who owned the list.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// The date and time when the todo item was removed.
    /// </summary>
    public DateTime RemovedAt { get; init; }
}
