using Base.Domain.Result;
using ToDoLists.Contracts;
using ToDoLists.Domain;

namespace ToDoLists.Application.Commands;

/// <summary>
/// Handles the UpdateToDoListTitleCommand to update existing todo list titles.
/// </summary>
public static class UpdateToDoListTitleCommandHandler
{
    public static class Codes
    {
        public const string UserNotAuthorized = "UpdateToDoListTitle.UserNotAuthorized";
    }

    /// <summary>
    /// Handles the command to update a todo list's title.
    /// </summary>
    /// <param name="command">The update todo list title command.</param>
    /// <param name="toDoListRepository">The todo list repository.</param>
    /// <param name="timeProvider">Time provider for consistent timestamps.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <returns>A Result containing the UpdateToDoListTitleResult if successful, or an error.</returns>
    public static async Task<Result<UpdateToDoListTitleResult>> Handle(
        UpdateToDoListTitleCommand command,
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

        DateTime now = timeProvider.GetUtcNow().UtcDateTime;
        Result updateResult = toDoList.UpdateTitle(command.Title, now);
        if (updateResult.IsFailure)
            return updateResult.Error;

        await toDoListRepository.UpdateAsync(toDoList, cancellationToken);
        await toDoListRepository.SaveChangesAsync(cancellationToken);

        return new UpdateToDoListTitleResult
        {
            ToDoListId = toDoList.Id.Value,
            Title = toDoList.Title.Value,
            UpdatedAt = toDoList.UpdatedAt ?? now
        };
    }

    private static Result<(ToDoListId toDoListId, UserId userId)> ValidateIds(UpdateToDoListTitleCommand command)
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
