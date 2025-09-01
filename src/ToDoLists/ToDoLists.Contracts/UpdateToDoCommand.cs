namespace ToDoLists.Contracts;

/// <summary>
/// Command to update a todo item's title and/or completion status.
/// </summary>
/// <param name="ToDoListId">The unique identifier of the todo list.</param>
/// <param name="ToDoId">The unique identifier of the todo item.</param>
/// <param name="UserId">The unique identifier of the user (for authorization).</param>
/// <param name="Title">The new title of the todo item (optional).</param>
/// <param name="IsCompleted">The new completion status (optional).</param>
public sealed record UpdateToDoCommand(
    string ToDoListId,
    string ToDoId,
    string UserId,
    string? Title,
    bool? IsCompleted);

/// <summary>
/// Result of successfully updating a todo item.
/// </summary>
/// <param name="Id">The unique identifier of the updated todo item.</param>
/// <param name="Title">The current title of the todo item.</param>
/// <param name="IsCompleted">Whether the todo item is completed.</param>
/// <param name="CreatedAt">The date and time when the todo item was created.</param>
/// <param name="CompletedAt">The date and time when the todo item was completed, if applicable.</param>
public sealed record UpdateToDoResult(
    Guid Id,
    string Title,
    bool IsCompleted,
    DateTime CreatedAt,
    DateTime? CompletedAt);
