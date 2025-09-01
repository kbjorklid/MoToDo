namespace ToDoLists.Contracts;

/// <summary>
/// Detailed information about a todo list including all its todos.
/// </summary>
/// <param name="Id">The unique identifier of the todo list.</param>
/// <param name="Title">The title of the todo list.</param>
/// <param name="Todos">Array of todos in the list.</param>
/// <param name="TodoCount">Number of todos in the list.</param>
/// <param name="CreatedAt">When the todo list was created.</param>
/// <param name="UpdatedAt">When the todo list was last updated.</param>
public sealed record ToDoListDetailDto(
    Guid Id,
    string Title,
    ToDoDto[] Todos,
    int TodoCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
