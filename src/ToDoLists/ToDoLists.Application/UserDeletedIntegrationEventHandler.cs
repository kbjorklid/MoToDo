using Base.Domain.Result;
using Microsoft.Extensions.Logging;
using ToDoLists.Domain;
using Users.Contracts;

namespace ToDoLists.Application;

/// <summary>
/// Handles the UserDeletedIntegrationEvent to cascade the deletion to all todo lists owned by the deleted user.
/// </summary>
public class UserDeletedIntegrationEventHandler
{
    /// <summary>
    /// Handles the UserDeletedIntegrationEvent using Wolverine's preferred static method pattern.
    /// Ensures domain events are published for each deleted todo list to maintain consistency.
    /// </summary>
    /// <param name="integrationEvent">The integration event containing the deleted user's ID.</param>
    /// <param name="toDoListRepository">The todo list repository.</param>
    /// <param name="timeProvider">Time provider for consistent timestamps.</param>
    /// <param name="logger">Logger for tracking deletion operations.</param>
    public static async Task Handle(
        UserDeletedIntegrationEvent integrationEvent,
        IToDoListRepository toDoListRepository,
        TimeProvider timeProvider,
        ILogger<UserDeletedIntegrationEventHandler> logger)
    {
        logger.LogInformation("Processing user deletion for user {UserId} - deleting associated todo lists", integrationEvent.UserId);

        Result<UserId> userIdResult = UserId.FromString(integrationEvent.UserId);
        if (userIdResult.IsFailure)
        {
            logger.LogWarning("Failed to parse UserId {UserId}: {Error}", integrationEvent.UserId, userIdResult.Error);
            return;
        }

        IReadOnlyList<ToDoList> userToDoLists = await toDoListRepository.GetByUserIdAsync(userIdResult.Value);

        if (userToDoLists.Count == 0)
        {
            logger.LogInformation("No todo lists found for user {UserId}", integrationEvent.UserId);
            return;
        }

        DateTime deletedAt = timeProvider.GetUtcNow().UtcDateTime;

        foreach (ToDoList toDoList in userToDoLists)
        {
            toDoList.MarkAsDeleted(deletedAt);
            await toDoListRepository.DeleteAsync(toDoList.Id);
        }

        await toDoListRepository.SaveChangesAsync();

        logger.LogInformation("Successfully deleted {DeletedCount} todo lists for user {UserId}", userToDoLists.Count, integrationEvent.UserId);
    }
}
