using Base.Domain;

namespace ToDoLists.Domain;

/// <summary>
/// Domain event raised when a ToDo item is marked as completed.
/// </summary>
/// <param name="ToDoListId">The unique identifier of the todo list.</param>
/// <param name="UserId">The unique identifier of the user who owns the list.</param>
/// <param name="ToDoId">The unique identifier of the completed todo item.</param>
/// <param name="OccurredOn">The date and time when the todo item was completed.</param>
public sealed record ToDoCompletedEvent(
    ToDoListId ToDoListId,
    UserId UserId,
    ToDoId ToDoId,
    DateTime OccurredOn) : IDomainEvent;
