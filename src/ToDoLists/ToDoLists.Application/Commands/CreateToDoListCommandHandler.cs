using Base.Domain.Result;
using ToDoLists.Contracts;
using ToDoLists.Domain;

namespace ToDoLists.Application.Commands;

/// <summary>
/// Handles the CreateToDoListCommand to create new todo lists in the system.
/// </summary>
public static class CreateToDoListCommandHandler
{
    /// <summary>
    /// Handles the command to create a new todo list using Wolverine's preferred static method pattern.
    /// </summary>
    /// <param name="command">The create todo list command containing user ID and title.</param>
    /// <param name="toDoListRepository">The todo list repository.</param>
    /// <param name="timeProvider">Time provider</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <returns>A Result containing the CreateToDoListResult if successful, or an error if validation fails.</returns>
    public static async Task<Result<CreateToDoListResult>> Handle(
        CreateToDoListCommand command,
        IToDoListRepository toDoListRepository,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        // Validate and convert UserId
        Result<UserId> userIdResult = UserId.FromString(command.UserId);
        if (userIdResult.IsFailure)
            return userIdResult.Error;

        // Create the ToDoList
        DateTime now = timeProvider.GetUtcNow().UtcDateTime;
        Result<ToDoList> toDoListResult = ToDoList.Create(userIdResult.Value, command.Title, now);
        if (toDoListResult.IsFailure)
            return toDoListResult.Error;

        ToDoList toDoList = toDoListResult.Value;

        // Save to repository
        await toDoListRepository.AddAsync(toDoList, cancellationToken);
        await toDoListRepository.SaveChangesAsync(cancellationToken);

        return new CreateToDoListResult(
            toDoList.Id.Value,
            toDoList.UserId.Value,
            toDoList.Title.Value,
            toDoList.CreatedAt
        );
    }
}
