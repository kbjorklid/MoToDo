namespace ToDoLists.Contracts;

/// <summary>
/// Command to delete an existing todo list.
/// </summary>
/// <param name="ToDoListId">The unique identifier of the todo list to delete.</param>
/// <param name="UserId">The unique identifier of the user (for authorization).</param>
public sealed record DeleteToDoListCommand(string ToDoListId, string UserId);

/// <summary>
/// Result of successfully deleting a todo list.
/// </summary>
/// <param name="ToDoListId">The unique identifier of the deleted todo list.</param>
/// <param name="UserId">The unique identifier of the user who owned the list.</param>
/// <param name="DeletedAt">The date and time when the todo list was deleted.</param>
public sealed record DeleteToDoListResult(Guid ToDoListId, Guid UserId, DateTime DeletedAt);
