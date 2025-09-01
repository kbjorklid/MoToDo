namespace ToDoLists.Contracts;

/// <summary>
/// Query to retrieve a specific todo list by its identifier.
/// </summary>
/// <param name="ToDoListId">The unique identifier of the todo list to retrieve.</param>
/// <param name="UserId">The unique identifier of the user making the request (for authorization).</param>
public sealed record GetToDoListQuery(string ToDoListId, string UserId);
