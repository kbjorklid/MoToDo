using Base.Domain;

namespace ToDoLists.Domain;

/// <summary>
/// Domain event raised when a ToDo item is successfully removed from a ToDoList.
/// </summary>
/// <param name="ToDoListId">The unique identifier of the todo list.</param>
/// <param name="UserId">The unique identifier of the user who owns the list.</param>
/// <param name="ToDoId">The unique identifier of the removed todo item.</param>
/// <param name="Title">The title of the removed todo item.</param>
/// <param name="OccurredOn">The date and time when the todo item was removed.</param>
public sealed record ToDoRemovedEvent(
    ToDoListId ToDoListId,
    UserId UserId,
    ToDoId ToDoId,
    Title Title,
    DateTime OccurredOn) : IDomainEvent;
