namespace ToDoLists.Contracts;

/// <summary>
/// Command to update the title of an existing todo list.
/// </summary>
public sealed record UpdateToDoListTitleCommand
{
    /// <summary>
    /// The unique identifier of the todo list to update.
    /// </summary>
    public required string ToDoListId { get; init; }

    /// <summary>
    /// The unique identifier of the user (for authorization).
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// The new title for the todo list.
    /// </summary>
    public required string Title { get; init; }
}

/// <summary>
/// Result of successfully updating a todo list title.
/// </summary>
public sealed record UpdateToDoListTitleResult
{
    /// <summary>
    /// The unique identifier of the updated todo list.
    /// </summary>
    public Guid ToDoListId { get; init; }

    /// <summary>
    /// The updated title of the todo list.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// When the todo list was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; init; }
}
