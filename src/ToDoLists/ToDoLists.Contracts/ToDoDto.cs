namespace ToDoLists.Contracts;

/// <summary>
/// Represents a todo item within a todo list for API responses.
/// </summary>
/// <param name="Id">The unique identifier of the todo item.</param>
/// <param name="Title">The title of the todo item.</param>
/// <param name="IsCompleted">Whether the todo item is completed.</param>
/// <param name="CreatedAt">When the todo item was created.</param>
/// <param name="CompletedAt">When the todo item was completed (null if not completed).</param>
public sealed record ToDoDto(
    Guid Id,
    string Title,
    bool IsCompleted,
    DateTime CreatedAt,
    DateTime? CompletedAt);
