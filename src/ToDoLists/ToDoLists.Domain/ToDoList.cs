using Base.Domain;
using Base.Domain.Result;

namespace ToDoLists.Domain;

/// <summary>
/// The ToDoList aggregate root represents a named collection of tasks owned by a specific user 
/// with enforced capacity limits and lifecycle management.
/// </summary>
public sealed class ToDoList : AggregateRoot<ToDoListId>
{
    public static class Codes
    {
        public const string MaxTodosExceeded = "ToDoList.MaxTodosExceeded";
        public const string DuplicateTitle = "ToDoList.DuplicateTitle";
        public const string ToDoNotFound = "ToDoList.ToDoNotFound";
        public const string NotFound = "ToDoList.NotFound";
    }

    private const int MAX_TODOS = 100;
    private readonly List<ToDo> _todos = new();

    public UserId UserId { get; private set; }
    public Title Title { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// Gets a read-only view of the todos in this list.
    /// </summary>
    public IReadOnlyList<ToDo> Todos => _todos.AsReadOnly();

    /// <summary>
    /// Gets the number of todos in this list.
    /// </summary>
    public int ToDoCount => _todos.Count;

    private ToDoList(ToDoListId id, UserId userId, Title title, DateTime createdAt) : base(id)
    {
        UserId = userId;
        Title = title;
        CreatedAt = createdAt;
        UpdatedAt = null;
    }

    /// <summary>
    /// Creates a new ToDoList with the specified details.
    /// </summary>
    /// <param name="userId">The unique identifier of the user who owns the list.</param>
    /// <param name="titleValue">The title of the todo list.</param>
    /// <param name="createdAt">The date and time when the list was created.</param>
    /// <returns>A Result containing the new ToDoList instance if successful, or an error if validation fails.</returns>
    public static Result<ToDoList> Create(UserId userId, string titleValue, DateTime createdAt)
    {
        Result<Title> titleResult = Title.Create(titleValue);
        if (titleResult.IsFailure)
            return titleResult.Error;

        var toDoListId = ToDoListId.New();
        var toDoList = new ToDoList(toDoListId, userId, titleResult.Value, createdAt);
        toDoList.AddDomainEvent(new ToDoListCreatedEvent(toDoListId, userId, titleResult.Value, createdAt));

        return toDoList;
    }

    /// <summary>
    /// Adds a new ToDo item to this list.
    /// </summary>
    /// <param name="titleValue">The title of the new todo item.</param>
    /// <param name="addedAt">The date and time when the todo item was added.</param>
    /// <returns>A Result containing the new ToDo instance if successful, or an error if validation fails.</returns>
    public Result<ToDo> AddToDo(string titleValue, DateTime addedAt)
    {
        if (_todos.Count >= MAX_TODOS)
            return new Error(Codes.MaxTodosExceeded, $"Cannot add more than {MAX_TODOS} todos to a list.", ErrorType.Validation);

        Result<Title> titleResult = Title.Create(titleValue);
        if (titleResult.IsFailure)
            return titleResult.Error;

        // Check for duplicate titles (case-insensitive)
        if (_todos.Any(t => string.Equals(t.Title.Value, titleResult.Value.Value, StringComparison.OrdinalIgnoreCase)))
            return new Error(Codes.DuplicateTitle, "A todo with this title already exists in the list.", ErrorType.Validation);

        var toDo = ToDo.Create(titleResult.Value, addedAt);
        _todos.Add(toDo);
        UpdatedAt = addedAt;

        AddDomainEvent(new ToDoAddedEvent(Id, UserId, toDo.Id, titleResult.Value, addedAt));

        return toDo;
    }

    /// <summary>
    /// Removes a ToDo item from this list.
    /// </summary>
    /// <param name="todoId">The unique identifier of the todo item to remove.</param>
    /// <param name="removedAt">The date and time when the todo item was removed.</param>
    /// <returns>A Result indicating success or failure.</returns>
    public Result RemoveToDo(ToDoId todoId, DateTime removedAt)
    {
        ToDo? toDo = _todos.FirstOrDefault(t => t.Id == todoId);
        if (toDo == null)
            return new Error(Codes.ToDoNotFound, "The specified todo item was not found in this list.", ErrorType.NotFound);

        _todos.Remove(toDo);
        UpdatedAt = removedAt;

        return Result.Success();
    }

    /// <summary>
    /// Updates the title of this todo list.
    /// </summary>
    /// <param name="newTitleValue">The new title for the todo list.</param>
    /// <param name="updatedAt">The date and time when the title was updated.</param>
    /// <returns>A Result indicating success or failure with error message.</returns>
    public Result UpdateTitle(string newTitleValue, DateTime updatedAt)
    {
        Result<Title> titleResult = Title.Create(newTitleValue);
        if (titleResult.IsFailure)
            return titleResult.Error;

        if (Title.Value != titleResult.Value.Value)
        {
            Title = titleResult.Value;
            UpdatedAt = updatedAt;
        }

        return Result.Success();
    }

    /// <summary>
    /// Marks a ToDo item in this list as completed.
    /// </summary>
    /// <param name="todoId">The unique identifier of the todo item to mark as completed.</param>
    /// <param name="completedAt">The date and time when the todo item was completed.</param>
    /// <returns>A Result indicating success or failure.</returns>
    public Result MarkToDoAsCompleted(ToDoId todoId, DateTime completedAt)
    {
        ToDo? toDo = _todos.FirstOrDefault(t => t.Id == todoId);
        if (toDo == null)
            return new Error(Codes.ToDoNotFound, "The specified todo item was not found in this list.", ErrorType.NotFound);

        toDo.MarkAsCompleted(completedAt);
        UpdatedAt = completedAt;

        AddDomainEvent(new ToDoCompletedEvent(Id, UserId, todoId, completedAt));

        return Result.Success();
    }

    /// <summary>
    /// Updates a ToDo item in this list (title and/or completion status).
    /// </summary>
    /// <param name="todoId">The unique identifier of the todo item to update.</param>
    /// <param name="newTitle">The new title for the todo item (optional).</param>
    /// <param name="isCompleted">The new completion status (optional).</param>
    /// <param name="updatedAt">The date and time when the todo item was updated.</param>
    /// <returns>A Result indicating success or failure.</returns>
    public Result UpdateToDo(ToDoId todoId, string? newTitle, bool? isCompleted, DateTime updatedAt)
    {
        ToDo? toDo = _todos.FirstOrDefault(t => t.Id == todoId);
        if (toDo == null)
            return new Error(Codes.ToDoNotFound, "The specified todo item was not found in this list.", ErrorType.NotFound);

        bool hasChanges = false;

        // Update title if provided
        if (newTitle != null)
        {
            Result<Title> titleResult = Title.Create(newTitle);
            if (titleResult.IsFailure)
                return titleResult.Error;

            // Check for duplicate titles (excluding current todo)
            if (_todos.Any(t => t.Id != todoId &&
                                string.Equals(t.Title.Value, titleResult.Value.Value, StringComparison.OrdinalIgnoreCase)))
            {
                return new Error(Codes.DuplicateTitle, "A todo with this title already exists in the list.", ErrorType.Validation);
            }

            Result updateTitleResult = toDo.UpdateTitle(titleResult.Value);
            if (updateTitleResult.IsFailure)
                return updateTitleResult.Error;

            hasChanges = true;
        }

        // Update completion status if provided
        if (isCompleted.HasValue)
        {
            if (isCompleted.Value && !toDo.IsCompleted)
            {
                toDo.MarkAsCompleted(updatedAt);
                AddDomainEvent(new ToDoCompletedEvent(Id, UserId, todoId, updatedAt));
                hasChanges = true;
            }
            else if (!isCompleted.Value && toDo.IsCompleted)
            {
                toDo.MarkAsIncomplete();
                hasChanges = true;
            }
        }

        if (hasChanges)
        {
            UpdatedAt = updatedAt;
        }

        return Result.Success();
    }
}
