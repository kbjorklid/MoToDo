namespace AiItemSuggestions.Domain;

/// <summary>
/// Repository interface for ToDoListSuggestions aggregate persistence and retrieval operations.
/// </summary>
public interface IToDoListSuggestionsRepository
{
    Task<ToDoListSuggestions?> GetByIdAsync(ToDoListSuggestionsId id, CancellationToken cancellationToken = default);

    Task<ToDoListSuggestions?> GetByToDoListIdAsync(ToDoListId toDoListId, CancellationToken cancellationToken = default);

    Task AddAsync(ToDoListSuggestions suggestions, CancellationToken cancellationToken = default);

    Task UpdateAsync(ToDoListSuggestions suggestions, CancellationToken cancellationToken = default);

    Task DeleteAsync(ToDoListSuggestionsId id, CancellationToken cancellationToken = default);

    Task<bool> ExistsForToDoListAsync(ToDoListId toDoListId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
