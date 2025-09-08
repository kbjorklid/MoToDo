namespace ToDoLists.Contracts;

/// <summary>
/// Command to create a new todo list for a user.
/// </summary>
public sealed record CreateToDoListCommand
{
    /// <summary>
    /// The unique identifier of the user who owns the list.
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// The title of the todo list.
    /// </summary>
    public required string Title { get; init; }
}

/// <summary>
/// Result of successfully creating a new todo list.
/// </summary>
public sealed record CreateToDoListResult
{
    /// <summary>
    /// The unique identifier of the created todo list.
    /// </summary>
    public Guid ToDoListId { get; init; }

    /// <summary>
    /// The unique identifier of the user who owns the list.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// The title of the todo list.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// The date and time when the todo list was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }
}
