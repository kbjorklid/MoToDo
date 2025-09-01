using Base.Domain.Result;
using ToDoLists.Contracts;
using ToDoLists.Domain;

namespace ToDoLists.Application.Commands;

/// <summary>
/// Handles the UpdateToDoCommand to update existing todo items.
/// </summary>
public static class UpdateToDoCommandHandler
{
    public static class Codes
    {
        public const string AccessDenied = "UpdateToDo.AccessDenied";
    }

    /// <summary>
    /// Handles the command to update a todo item's title and/or completion status.
    /// </summary>
    /// <param name="command">The update todo command.</param>
    /// <param name="toDoListRepository">The todo list repository.</param>
    /// <param name="timeProvider">Time provider for consistent timestamps.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <returns>A Result containing the UpdateToDoResult if successful, or an error.</returns>
    public static async Task<Result<UpdateToDoResult>> Handle(
        UpdateToDoCommand command,
        IToDoListRepository toDoListRepository,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        // Validate and convert IDs
        Result<ToDoListId> toDoListIdResult = ToDoListId.FromString(command.ToDoListId);
        if (toDoListIdResult.IsFailure)
            return toDoListIdResult.Error;

        Result<ToDoId> toDoIdResult = ToDoId.FromString(command.ToDoId);
        if (toDoIdResult.IsFailure)
            return toDoIdResult.Error;

        Result<UserId> userIdResult = UserId.FromString(command.UserId);
        if (userIdResult.IsFailure)
            return userIdResult.Error;

        // Load the existing todo list
        ToDoList? toDoList = await toDoListRepository.GetByIdAsync(toDoListIdResult.Value, cancellationToken);
        if (toDoList == null)
            return new Error(ToDoList.Codes.NotFound, "The specified todo list was not found.", ErrorType.NotFound);

        // Check authorization - user must own the list
        if (toDoList.UserId != userIdResult.Value)
            return new Error(Codes.AccessDenied, "Access denied to this todo list.", ErrorType.Forbidden);

        // Update the todo item
        DateTime now = timeProvider.GetUtcNow().UtcDateTime;
        Result updateResult = toDoList.UpdateToDo(toDoIdResult.Value, command.Title, command.IsCompleted, now);
        if (updateResult.IsFailure)
            return updateResult.Error;

        // Get the updated todo for the response
        ToDo? updatedToDo = toDoList.Todos.FirstOrDefault(t => t.Id == toDoIdResult.Value);
        if (updatedToDo == null)
            return new Error(ToDoList.Codes.ToDoNotFound, "The specified todo item was not found in this list.", ErrorType.NotFound);

        // Save changes
        await toDoListRepository.UpdateAsync(toDoList, cancellationToken);
        await toDoListRepository.SaveChangesAsync(cancellationToken);

        // Map to result
        return new UpdateToDoResult(
            updatedToDo.Id.Value,
            updatedToDo.Title.Value,
            updatedToDo.IsCompleted,
            updatedToDo.CreatedAt,
            updatedToDo.CompletedAt
        );
    }
}
