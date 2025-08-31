using Base.Domain;
using Base.Domain.Result;

namespace ToDoLists.Domain;

/// <summary>
/// An individual task item within a todo list with completion tracking and title management capabilities.
/// </summary>
public sealed class ToDo : Entity<ToDoId>
{
    public Title Title { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private ToDo(ToDoId id, Title title, DateTime createdAt) : base(id)
    {
        Title = title;
        IsCompleted = false;
        CreatedAt = createdAt;
        CompletedAt = null;
    }

    /// <summary>
    /// Creates a new ToDo item with the specified title.
    /// </summary>
    /// <param name="title">The title of the todo item.</param>
    /// <param name="createdAt">The date and time when the todo item was created.</param>
    /// <returns>A new ToDo instance.</returns>
    public static ToDo Create(Title title, DateTime createdAt)
    {
        var toDoId = ToDoId.New();
        return new ToDo(toDoId, title, createdAt);
    }

    /// <summary>
    /// Marks the todo item as completed.
    /// </summary>
    /// <param name="completedAt">The date and time when the todo item was completed.</param>
    internal void MarkAsCompleted(DateTime completedAt)
    {
        if (!IsCompleted)
        {
            IsCompleted = true;
            CompletedAt = completedAt;
        }
    }

    /// <summary>
    /// Marks the todo item as incomplete.
    /// </summary>
    internal void MarkAsIncomplete()
    {
        if (IsCompleted)
        {
            IsCompleted = false;
            CompletedAt = null;
        }
    }

    /// <summary>
    /// Updates the title of the todo item.
    /// </summary>
    /// <param name="newTitle">The new title for the todo item.</param>
    /// <returns>A Result indicating success or failure with error message.</returns>
    internal Result UpdateTitle(Title newTitle)
    {
        if (Title.Value != newTitle.Value)
            Title = newTitle;

        return Result.Success();
    }
}
