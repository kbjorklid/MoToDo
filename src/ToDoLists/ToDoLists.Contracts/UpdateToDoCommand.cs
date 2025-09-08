namespace ToDoLists.Contracts;

/// <summary>
/// Command to update a todo item's title and/or completion status.
/// </summary>
public sealed record UpdateToDoCommand
{
    /// <summary>
    /// The unique identifier of the todo list.
    /// </summary>
    public required string ToDoListId { get; init; }

    /// <summary>
    /// The unique identifier of the todo item.
    /// </summary>
    public required string ToDoId { get; init; }

    /// <summary>
    /// The unique identifier of the user (for authorization).
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// The new title of the todo item (optional).
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// The new completion status (optional).
    /// </summary>
    public bool? IsCompleted { get; init; }
}

/// <summary>
/// Result of successfully updating a todo item.
/// </summary>
public sealed record UpdateToDoResult
{
    /// <summary>
    /// The unique identifier of the updated todo item.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// The current title of the todo item.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Whether the todo item is completed.
    /// </summary>
    public bool IsCompleted { get; init; }

    /// <summary>
    /// The date and time when the todo item was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The date and time when the todo item was completed, if applicable.
    /// </summary>
    public DateTime? CompletedAt { get; init; }
}
