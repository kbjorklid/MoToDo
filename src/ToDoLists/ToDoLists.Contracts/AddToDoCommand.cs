namespace ToDoLists.Contracts;

/// <summary>
/// Command to add a new todo item to an existing todo list.
/// </summary>
public sealed record AddToDoCommand
{
    /// <summary>
    /// The unique identifier of the todo list.
    /// </summary>
    public required string ToDoListId { get; init; }

    /// <summary>
    /// The title of the todo item.
    /// </summary>
    public required string Title { get; init; }
}

/// <summary>
/// Result of successfully adding a new todo item to a list.
/// </summary>
public sealed record AddToDoResult
{
    /// <summary>
    /// The unique identifier of the created todo item.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// The title of the todo item.
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
