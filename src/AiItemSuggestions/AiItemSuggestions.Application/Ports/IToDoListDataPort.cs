using AiItemSuggestions.Domain;

namespace AiItemSuggestions.Application.Ports;

/// <summary>
/// Port interface for fetching read-only ToDoList data from the ToDoLists module.
/// Provides access to todo list data converted to snapshot format for AI processing.
/// </summary>
public interface IToDoListDataPort
{
    /// <summary>
    /// Retrieves a todo list and its items as an immutable snapshot.
    /// </summary>
    /// <param name="toDoListId">The unique identifier of the todo list to fetch.</param>
    /// <param name="userId">The user ID for authorization (required by ToDoLists module).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A ToDoListSnapshot if the list exists and user has access, null otherwise.</returns>
    Task<ToDoListSnapshot?> GetToDoListSnapshotAsync(ToDoListId toDoListId, string userId, CancellationToken cancellationToken = default);
}
