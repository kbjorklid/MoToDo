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
        public const string UserNotAuthorized = "UpdateToDo.UserNotAuthorized";
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

        DateTime now = timeProvider.GetUtcNow().UtcDateTime;
        Result updateResult = toDoList.UpdateToDo(toDoId, command.Title, command.IsCompleted, now);
        if (updateResult.IsFailure)
            return updateResult.Error;

        ToDo? updatedToDo = toDoList.Todos.FirstOrDefault(t => t.Id == toDoId);
        if (updatedToDo == null)
            return new Error(ToDoList.Codes.ToDoNotFound, "The specified todo item was not found in this list.", ErrorType.NotFound);

        await toDoListRepository.UpdateAsync(toDoList, cancellationToken);
        await toDoListRepository.SaveChangesAsync(cancellationToken);

        return new UpdateToDoResult
        {
            Id = updatedToDo.Id.Value,
            Title = updatedToDo.Title.Value,
            IsCompleted = updatedToDo.IsCompleted,
            CreatedAt = updatedToDo.CreatedAt,
            CompletedAt = updatedToDo.CompletedAt
        };
    }

    private static Result<(ToDoListId toDoListId, ToDoId toDoId, UserId userId)> ValidateIds(UpdateToDoCommand command)
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
