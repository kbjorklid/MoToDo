namespace ToDoLists.Contracts;

/// <summary>
/// Command to update the title of an existing todo list.
/// </summary>
/// <param name="ToDoListId">The unique identifier of the todo list to update.</param>
/// <param name="UserId">The unique identifier of the user (for authorization).</param>
/// <param name="Title">The new title for the todo list.</param>
public sealed record UpdateToDoListTitleCommand(string ToDoListId, string UserId, string Title);

/// <summary>
/// Result of successfully updating a todo list title.
/// </summary>
/// <param name="ToDoListId">The unique identifier of the updated todo list.</param>
/// <param name="Title">The updated title of the todo list.</param>
/// <param name="UpdatedAt">When the todo list was last updated.</param>
public sealed record UpdateToDoListTitleResult(Guid ToDoListId, string Title, DateTime UpdatedAt);
