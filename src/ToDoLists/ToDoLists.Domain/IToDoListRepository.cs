namespace ToDoLists.Domain;

/// <summary>
/// Repository interface for ToDoList aggregate persistence and retrieval operations.
/// </summary>
public interface IToDoListRepository
{
    /// <summary>
    /// Retrieves a ToDoList by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the todo list.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ToDoList if found, null otherwise.</returns>
    Task<ToDoList?> GetByIdAsync(ToDoListId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all ToDoLists owned by a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only list of ToDoLists owned by the user.</returns>
    Task<IReadOnlyList<ToDoList>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new ToDoList to the repository.
    /// </summary>
    /// <param name="toDoList">The ToDoList to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(ToDoList toDoList, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing ToDoList in the repository.
    /// </summary>
    /// <param name="toDoList">The ToDoList to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateAsync(ToDoList toDoList, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a ToDoList from the repository.
    /// </summary>
    /// <param name="id">The unique identifier of the todo list to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync(ToDoListId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a ToDoList exists with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the ToDoList exists, false otherwise.</returns>
    Task<bool> ExistsAsync(ToDoListId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes to the underlying data store.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
