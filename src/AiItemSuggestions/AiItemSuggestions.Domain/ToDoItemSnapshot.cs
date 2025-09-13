using Base.Domain.Result;

namespace AiItemSuggestions.Domain;

/// <summary>
/// Represents a minimal snapshot of a todo item from the ToDoLists domain,
/// containing only the essential data needed for AI item suggestions.
/// This is an immutable value object that maintains module boundaries.
/// </summary>
public readonly record struct ToDoItemSnapshot
{
    public static class Codes
    {
        public const string EmptyTitle = "ToDoItemSnapshot.EmptyTitle";
    }

    public ToDoId Id { get; }
    public string Title { get; }

    private ToDoItemSnapshot(ToDoId id, string title)
    {
        Id = id;
        Title = title;
    }

    public static Result<ToDoItemSnapshot> Create(ToDoId id, string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return new Error(Codes.EmptyTitle, "Todo item title cannot be empty.", ErrorType.Validation);

        return new ToDoItemSnapshot(id, title.Trim());
    }
}
