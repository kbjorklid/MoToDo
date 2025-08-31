using Base.Domain;

namespace ToDoLists.Domain;

/// <summary>
/// Domain event raised when a new ToDoList is successfully created.
/// </summary>
/// <param name="ToDoListId">The unique identifier of the created todo list.</param>
/// <param name="UserId">The unique identifier of the user who owns the list.</param>
/// <param name="Title">The title of the created todo list.</param>
/// <param name="OccurredOn">The date and time when the todo list was created.</param>
public sealed record ToDoListCreatedEvent(
    ToDoListId ToDoListId,
    UserId UserId,
    Title Title,
    DateTime OccurredOn) : IDomainEvent;
