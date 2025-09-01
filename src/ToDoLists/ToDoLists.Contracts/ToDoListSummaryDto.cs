namespace ToDoLists.Contracts;

/// <summary>
/// Summary information about a todo list for list views.
/// </summary>
/// <param name="Id">The unique identifier of the todo list.</param>
/// <param name="Title">The title of the todo list.</param>
/// <param name="TodoCount">Number of todos in the list.</param>
/// <param name="CreatedAt">When the todo list was created.</param>
/// <param name="UpdatedAt">When the todo list was last updated.</param>
public sealed record ToDoListSummaryDto(
    Guid Id,
    string Title,
    int TodoCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
