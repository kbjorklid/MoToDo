using Base.Domain.Result;
using ToDoLists.Contracts;
using ToDoLists.Domain;

namespace ToDoLists.Application.Commands;

/// <summary>
/// Handles the DeleteToDoListCommand to delete existing todo lists.
/// </summary>
public static class DeleteToDoListCommandHandler
{
    public static class Codes
    {
        public const string UserNotAuthorized = "DeleteToDoList.UserNotAuthorized";
    }

    /// <summary>
    /// Handles the command to delete a todo list.
    /// </summary>
    /// <param name="command">The delete todo list command.</param>
    /// <param name="toDoListRepository">The todo list repository.</param>
    /// <param name="timeProvider">Time provider for consistent timestamps.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <returns>A Result containing the DeleteToDoListResult if successful, or an error.</returns>
    public static async Task<Result<DeleteToDoListResult>> Handle(
        DeleteToDoListCommand command,
        IToDoListRepository toDoListRepository,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        Result<(ToDoListId toDoListId, UserId userId)> validationResult = ValidateIds(command);
        if (validationResult.IsFailure)
            return validationResult.Error;

        (ToDoListId toDoListId, UserId userId) = validationResult.Value;

        ToDoList? toDoList = await toDoListRepository.GetByIdAsync(toDoListId, cancellationToken);
        if (toDoList == null)
            return new Error(ToDoList.Codes.NotFound, "The specified todo list was not found.", ErrorType.NotFound);

        Result authorizationResult = CheckAuthorization(toDoList, userId);
        if (authorizationResult.IsFailure)
            return authorizationResult.Error;

        DateTime deletedAt = timeProvider.GetUtcNow().UtcDateTime;
        Result deleteResult = toDoList.MarkAsDeleted(deletedAt);
        if (deleteResult.IsFailure)
            return deleteResult.Error;

        await toDoListRepository.DeleteAsync(toDoListId, cancellationToken);
        await toDoListRepository.SaveChangesAsync(cancellationToken);

        return new DeleteToDoListResult(
            toDoList.Id.Value,
            toDoList.UserId.Value,
            deletedAt
        );
    }

    private static Result<(ToDoListId toDoListId, UserId userId)> ValidateIds(DeleteToDoListCommand command)
    {
        Result<ToDoListId> toDoListIdResult = ToDoListId.FromString(command.ToDoListId);
        if (toDoListIdResult.IsFailure)
            return toDoListIdResult.Error;

        Result<UserId> userIdResult = UserId.FromString(command.UserId);
        if (userIdResult.IsFailure)
            return userIdResult.Error;

        return (toDoListIdResult.Value, userIdResult.Value);
    }

    private static Result CheckAuthorization(ToDoList toDoList, UserId userId)
    {
        if (toDoList.UserId != userId)
            return new Error(Codes.UserNotAuthorized, "Access denied to this todo list.", ErrorType.Forbidden);

        return Result.Success();
    }
}
