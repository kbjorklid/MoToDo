using AiItemSuggestions.Domain;
using Microsoft.EntityFrameworkCore;

namespace AiItemSuggestions.Infrastructure;

/// <summary>
/// Entity Framework implementation of the IToDoListSuggestionsRepository for ToDoListSuggestions aggregate persistence.
/// </summary>
internal sealed class ToDoListSuggestionsRepository : IToDoListSuggestionsRepository
{
    private readonly AiItemSuggestionsDbContext _context;

    public ToDoListSuggestionsRepository(AiItemSuggestionsDbContext context)
    {
        _context = context;
    }

    public async Task<ToDoListSuggestions?> GetByIdAsync(ToDoListSuggestionsId id, CancellationToken cancellationToken = default)
    {
        return await _context.ToDoListSuggestions
            .FirstOrDefaultAsync(tls => tls.Id == id, cancellationToken);
    }

    public async Task<ToDoListSuggestions?> GetByToDoListIdAsync(ToDoListId toDoListId, CancellationToken cancellationToken = default)
    {
        return await _context.ToDoListSuggestions
            .FirstOrDefaultAsync(tls => tls.ToDoListId == toDoListId, cancellationToken);
    }

    public async Task AddAsync(ToDoListSuggestions suggestions, CancellationToken cancellationToken = default)
    {
        await _context.ToDoListSuggestions.AddAsync(suggestions, cancellationToken);
    }

    public Task UpdateAsync(ToDoListSuggestions suggestions, CancellationToken cancellationToken = default)
    {
        _context.ToDoListSuggestions.Update(suggestions);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(ToDoListSuggestionsId id, CancellationToken cancellationToken = default)
    {
        ToDoListSuggestions? suggestions = await _context.ToDoListSuggestions.FindAsync(new object[] { id }, cancellationToken);
        if (suggestions != null)
        {
            _context.ToDoListSuggestions.Remove(suggestions);
        }
    }

    public async Task<bool> ExistsForToDoListAsync(ToDoListId toDoListId, CancellationToken cancellationToken = default)
    {
        return await _context.ToDoListSuggestions.AnyAsync(tls => tls.ToDoListId == toDoListId, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
