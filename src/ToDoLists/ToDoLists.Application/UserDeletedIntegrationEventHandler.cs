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
    /// </summary>
    /// <param name="integrationEvent">The integration event containing the deleted user's ID.</param>
    /// <param name="toDoListRepository">The todo list repository.</param>
    /// <param name="logger">Logger for tracking deletion operations.</param>
    public static async Task Handle(
        UserDeletedIntegrationEvent integrationEvent,
        IToDoListRepository toDoListRepository,
        ILogger<UserDeletedIntegrationEventHandler> logger)
    {
        logger.LogInformation("Processing user deletion for user {UserId} - deleting associated todo lists", integrationEvent.UserId);

        Result<UserId> userIdResult = UserId.FromString(integrationEvent.UserId);
        if (userIdResult.IsFailure)
        {
            logger.LogWarning("Failed to parse UserId {UserId}: {Error}", integrationEvent.UserId, userIdResult.Error);
            return;
        }

        int deletedCount = await toDoListRepository.DeleteByUserIdAsync(userIdResult.Value);
        await toDoListRepository.SaveChangesAsync();

        logger.LogInformation("Successfully deleted {DeletedCount} todo lists for user {UserId}", deletedCount, integrationEvent.UserId);
    }
}
