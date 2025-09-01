namespace ToDoLists.Domain;

/// <summary>
/// Defines the available sorting options for ToDoList queries.
/// </summary>
public enum ToDoListsSortBy
{
    /// <summary>
    /// Sort by the date and time the todo list was created.
    /// </summary>
    CreatedAt,

    /// <summary>
    /// Sort by the todo list title.
    /// </summary>
    Title
}
