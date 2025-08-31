using Base.Domain.Result;
using ToDoLists.Contracts;
using ToDoLists.Domain;

namespace ToDoLists.Application.Commands;

/// <summary>
/// Handles the AddToDoCommand to add new todo items to existing todo lists.
/// </summary>
public static class AddToDoCommandHandler
{
    /// <summary>
    /// Handles the command to add a new todo item to an existing todo list.
    /// </summary>
    /// <param name="command">The add todo command containing the list ID and title.</param>
    /// <param name="toDoListRepository">The todo list repository.</param>
    /// <param name="timeProvider">Time provider for consistent timestamps.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <returns>A Result containing the AddToDoResult if successful, or an error if validation fails or the list is not found.</returns>
    public static async Task<Result<AddToDoResult>> Handle(
        AddToDoCommand command,
        IToDoListRepository toDoListRepository,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        // Validate and convert ToDoListId
        Result<ToDoListId> toDoListIdResult = ToDoListId.FromString(command.ToDoListId);
        if (toDoListIdResult.IsFailure)
            return toDoListIdResult.Error;

        // Load the existing todo list
        ToDoList? toDoList = await toDoListRepository.GetByIdAsync(toDoListIdResult.Value, cancellationToken);
        if (toDoList == null)
            return new Error(ToDoList.Codes.NotFound, "The specified todo list was not found.", ErrorType.NotFound);

        // Add the new todo item
        DateTime now = timeProvider.GetUtcNow().UtcDateTime;
        Result<ToDo> addToDoResult = toDoList.AddToDo(command.Title, now);
        if (addToDoResult.IsFailure)
            return addToDoResult.Error;

        ToDo newToDo = addToDoResult.Value;

        // Save changes
        await toDoListRepository.UpdateAsync(toDoList, cancellationToken);
        await toDoListRepository.SaveChangesAsync(cancellationToken);

        // Map to result
        return new AddToDoResult(
            newToDo.Id.Value,
            newToDo.Title.Value,
            newToDo.IsCompleted,
            newToDo.CreatedAt,
            newToDo.CompletedAt
        );
    }
}
