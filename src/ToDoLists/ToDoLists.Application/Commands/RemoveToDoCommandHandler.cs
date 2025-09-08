using Base.Domain.Result;
using ToDoLists.Contracts;
using ToDoLists.Domain;

namespace ToDoLists.Application.Commands;

/// <summary>
/// Handles the RemoveToDoCommand to remove todo items from existing todo lists.
/// </summary>
public static class RemoveToDoCommandHandler
{
    public static class Codes
    {
        public const string UserNotAuthorized = "RemoveToDo.UserNotAuthorized";
    }

    /// <summary>
    /// Handles the command to remove a todo item from an existing todo list.
    /// </summary>
    /// <param name="command">The remove todo command containing the list ID, todo ID, and user ID.</param>
    /// <param name="toDoListRepository">The todo list repository.</param>
    /// <param name="timeProvider">Time provider for consistent timestamps.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <returns>A Result containing the RemoveToDoResult if successful, or an error if validation fails or the list/todo is not found.</returns>
    public static async Task<Result<RemoveToDoResult>> Handle(
        RemoveToDoCommand command,
        IToDoListRepository toDoListRepository,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        Result<(ToDoListId toDoListId, ToDoId toDoId, UserId userId)> validationResult = ValidateIds(command);
        if (validationResult.IsFailure)
            return validationResult.Error;

        (ToDoListId toDoListId, ToDoId toDoId, UserId userId) = validationResult.Value;

        ToDoList? toDoList = await toDoListRepository.GetByIdAsync(toDoListId, cancellationToken);
        if (toDoList == null)
            return new Error(ToDoList.Codes.NotFound, "The specified todo list was not found.", ErrorType.NotFound);

        Result authorizationResult = CheckAuthorization(toDoList, userId);
        if (authorizationResult.IsFailure)
            return authorizationResult.Error;

        DateTime removedAt = timeProvider.GetUtcNow().UtcDateTime;
        Result removeResult = toDoList.RemoveToDo(toDoId, removedAt);
        if (removeResult.IsFailure)
            return removeResult.Error;

        await toDoListRepository.UpdateAsync(toDoList, cancellationToken);
        await toDoListRepository.SaveChangesAsync(cancellationToken);

        return new RemoveToDoResult
        {
            ToDoListId = toDoListId.Value,
            ToDoId = toDoId.Value,
            UserId = userId.Value,
            RemovedAt = removedAt
        };
    }

    private static Result<(ToDoListId toDoListId, ToDoId toDoId, UserId userId)> ValidateIds(RemoveToDoCommand command)
    {
        Result<ToDoListId> toDoListIdResult = ToDoListId.FromString(command.ToDoListId);
        if (toDoListIdResult.IsFailure)
            return toDoListIdResult.Error;

        Result<ToDoId> toDoIdResult = ToDoId.FromString(command.ToDoId);
        if (toDoIdResult.IsFailure)
            return toDoIdResult.Error;

        Result<UserId> userIdResult = UserId.FromString(command.UserId);
        if (userIdResult.IsFailure)
            return userIdResult.Error;

        return (toDoListIdResult.Value, toDoIdResult.Value, userIdResult.Value);
    }

    private static Result CheckAuthorization(ToDoList toDoList, UserId userId)
    {
        if (toDoList.UserId != userId)
            return new Error(Codes.UserNotAuthorized, "Access denied to this todo list.", ErrorType.Forbidden);

        return Result.Success();
    }
}
