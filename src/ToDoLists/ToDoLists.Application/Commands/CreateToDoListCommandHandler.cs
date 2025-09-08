using Base.Domain.Result;
using ToDoLists.Contracts;
using ToDoLists.Domain;
using Users.Contracts;
using Wolverine;

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
    /// <param name="messageBus">Message bus for inter-module communication</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <returns>A Result containing the CreateToDoListResult if successful, or an error if validation fails.</returns>
    public static async Task<Result<CreateToDoListResult>> Handle(
        CreateToDoListCommand command,
        IToDoListRepository toDoListRepository,
        TimeProvider timeProvider,
        IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        Result<UserId> userIdResult = UserId.FromString(command.UserId);
        if (userIdResult.IsFailure)
            return userIdResult.Error;

        GetUserByIdQuery getUserQuery = new()
        {
            UserId = command.UserId
        };
        Result<UserDto> userResult = await messageBus.InvokeAsync<Result<UserDto>>(getUserQuery, cancellationToken);
        if (userResult.IsFailure)
            return userResult.Error;

        DateTime now = timeProvider.GetUtcNow().UtcDateTime;
        Result<ToDoList> toDoListResult = ToDoList.Create(userIdResult.Value, command.Title, now);
        if (toDoListResult.IsFailure)
            return toDoListResult.Error;

        ToDoList toDoList = toDoListResult.Value;

        await toDoListRepository.AddAsync(toDoList, cancellationToken);
        await toDoListRepository.SaveChangesAsync(cancellationToken);

        return new CreateToDoListResult
        {
            ToDoListId = toDoList.Id.Value,
            UserId = toDoList.UserId.Value,
            Title = toDoList.Title.Value,
            CreatedAt = toDoList.CreatedAt
        };
    }
}
