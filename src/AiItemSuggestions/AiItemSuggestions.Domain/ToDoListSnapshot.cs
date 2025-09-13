using Base.Domain;
using Base.Domain.Result;

namespace AiItemSuggestions.Domain;

/// <summary>
/// Represents an immutable snapshot of a todo list from the ToDoLists domain,
/// containing only the essential data needed for AI item suggestions.
/// This aggregate root maintains module boundaries while providing access to todo list data.
/// </summary>
public sealed class ToDoListSnapshot : AggregateRoot<ToDoListId>
{
    public static class Codes
    {
        public const string EmptyTitle = "ToDoListSnapshot.EmptyTitle";
    }

    private readonly List<ToDoItemSnapshot> _items;

    public string Title { get; private set; }

    /// <summary>
    /// Gets a read-only view of the todo items in this snapshot.
    /// </summary>
    public IReadOnlyList<ToDoItemSnapshot> Items => _items.AsReadOnly();

    private ToDoListSnapshot(
        ToDoListId id,
        string title,
        IEnumerable<ToDoItemSnapshot> items) : base(id)
    {
        Title = title;
        _items = items.ToList();
    }

    /// <summary>
    /// Creates a new immutable snapshot of a todo list.
    /// </summary>
    /// <param name="toDoListId">The unique identifier of the todo list.</param>
    /// <param name="title">The title of the todo list.</param>
    /// <param name="items">The collection of todo items in the list.</param>
    /// <returns>A Result containing the new ToDoListSnapshot instance if successful, or an error if validation fails.</returns>
    public static Result<ToDoListSnapshot> Create(
        ToDoListId toDoListId,
        string title,
        IEnumerable<ToDoItemSnapshot> items)
    {
        if (string.IsNullOrWhiteSpace(title))
            return new Error(Codes.EmptyTitle, "Todo list title cannot be empty.", ErrorType.Validation);

        return new ToDoListSnapshot(toDoListId, title.Trim(), items);
    }
}
