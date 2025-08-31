namespace ToDoLists.Contracts;

/// <summary>
/// Command to add a new todo item to an existing todo list.
/// </summary>
/// <param name="ToDoListId">The unique identifier of the todo list.</param>
/// <param name="Title">The title of the todo item.</param>
public sealed record AddToDoCommand(string ToDoListId, string Title);

/// <summary>
/// Result of successfully adding a new todo item to a list.
/// </summary>
/// <param name="Id">The unique identifier of the created todo item.</param>
/// <param name="Title">The title of the todo item.</param>
/// <param name="IsCompleted">Whether the todo item is completed.</param>
/// <param name="CreatedAt">The date and time when the todo item was created.</param>
/// <param name="CompletedAt">The date and time when the todo item was completed, if applicable.</param>
public sealed record AddToDoResult(Guid Id, string Title, bool IsCompleted, DateTime CreatedAt, DateTime? CompletedAt);
