namespace ToDoLists.Contracts;

/// <summary>
/// Command to remove a todo item from an existing todo list.
/// </summary>
/// <param name="ToDoListId">The unique identifier of the todo list.</param>
/// <param name="ToDoId">The unique identifier of the todo item to remove.</param>
/// <param name="UserId">The unique identifier of the user (for authorization).</param>
public sealed record RemoveToDoCommand(string ToDoListId, string ToDoId, string UserId);

/// <summary>
/// Result of successfully removing a todo item from a list.
/// </summary>
/// <param name="ToDoListId">The unique identifier of the todo list.</param>
/// <param name="ToDoId">The unique identifier of the removed todo item.</param>
/// <param name="UserId">The unique identifier of the user who owned the list.</param>
/// <param name="RemovedAt">The date and time when the todo item was removed.</param>
public sealed record RemoveToDoResult(Guid ToDoListId, Guid ToDoId, Guid UserId, DateTime RemovedAt);
