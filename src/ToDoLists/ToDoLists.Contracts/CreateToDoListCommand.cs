namespace ToDoLists.Contracts;

/// <summary>
/// Command to create a new todo list for a user.
/// </summary>
/// <param name="UserId">The unique identifier of the user who owns the list.</param>
/// <param name="Title">The title of the todo list.</param>
public sealed record CreateToDoListCommand(string UserId, string Title);

/// <summary>
/// Result of successfully creating a new todo list.
/// </summary>
/// <param name="ToDoListId">The unique identifier of the created todo list.</param>
/// <param name="UserId">The unique identifier of the user who owns the list.</param>
/// <param name="Title">The title of the todo list.</param>
/// <param name="CreatedAt">The date and time when the todo list was created.</param>
public sealed record CreateToDoListResult(Guid ToDoListId, Guid UserId, string Title, DateTime CreatedAt);
