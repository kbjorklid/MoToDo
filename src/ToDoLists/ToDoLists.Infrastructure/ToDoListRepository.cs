using Microsoft.EntityFrameworkCore;
using ToDoLists.Domain;

namespace ToDoLists.Infrastructure;

/// <summary>
/// Entity Framework implementation of the IToDoListRepository for ToDoList aggregate persistence.
/// </summary>
internal sealed class ToDoListRepository : IToDoListRepository
{
    private readonly ToDoListsDbContext _context;

    public ToDoListRepository(ToDoListsDbContext context)
    {
        _context = context;
    }

    public async Task<ToDoList?> GetByIdAsync(ToDoListId id, CancellationToken cancellationToken = default)
    {
        return await _context.ToDoLists
            .Include(tl => tl.Todos)
            .FirstOrDefaultAsync(tl => tl.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ToDoList>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        List<ToDoList> lists = await _context.ToDoLists
            .Include(tl => tl.Todos)
            .Where(tl => tl.UserId == userId)
            .OrderByDescending(tl => tl.CreatedAt)
            .ToListAsync(cancellationToken);

        return lists.AsReadOnly();
    }

    public async Task AddAsync(ToDoList toDoList, CancellationToken cancellationToken = default)
    {
        await _context.ToDoLists.AddAsync(toDoList, cancellationToken);
    }

    public Task UpdateAsync(ToDoList toDoList, CancellationToken cancellationToken = default)
    {
        _context.ToDoLists.Update(toDoList);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(ToDoListId id, CancellationToken cancellationToken = default)
    {
        ToDoList? toDoList = await _context.ToDoLists.FindAsync(new object[] { id }, cancellationToken);
        if (toDoList != null)
        {
            _context.ToDoLists.Remove(toDoList);
        }
    }

    public async Task<bool> ExistsAsync(ToDoListId id, CancellationToken cancellationToken = default)
    {
        return await _context.ToDoLists.AnyAsync(tl => tl.Id == id, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
