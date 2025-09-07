using Base.Domain;

namespace ToDoLists.Domain;

/// <summary>
/// Domain event raised when a ToDoList is successfully deleted.
/// </summary>
/// <param name="ToDoListId">The unique identifier of the deleted todo list.</param>
/// <param name="UserId">The unique identifier of the user who owned the list.</param>
/// <param name="Title">The title of the deleted todo list.</param>
/// <param name="DeletedAt">The date and time when the todo list was deleted.</param>
/// <param name="OccurredOn">The date and time when the domain event occurred.</param>
public sealed record ToDoListDeletedEvent(
    ToDoListId ToDoListId,
    UserId UserId,
    Title Title,
    DateTime DeletedAt,
    DateTime OccurredOn) : IDomainEvent;
