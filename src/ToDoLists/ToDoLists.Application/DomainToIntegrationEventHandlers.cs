using Microsoft.Extensions.Logging;
using ToDoLists.Contracts;
using ToDoLists.Domain;
using Wolverine;

namespace ToDoLists.Application;

/// <summary>
/// Contains handlers that convert domain events to integration events for inter-module communication.
/// </summary>
public class DomainToIntegrationEventHandler
{
    public static async Task Handle(ToDoAddedEvent domainEvent, IMessageBus bus, ILogger<DomainToIntegrationEventHandler> logger)
    {
        logger.LogDebug("Converting domain event to integration event for TodoList {ToDoListId}, Todo {ToDoId}",
            domainEvent.ToDoListId, domainEvent.ToDoId);

        ToDoAddedIntegrationEvent integrationEvent = new(
            domainEvent.OccurredOn,
            domainEvent.ToDoListId.ToString(),
            domainEvent.ToDoId.ToString(),
            domainEvent.UserId.ToString());

        await bus.PublishAsync(integrationEvent);

        logger.LogDebug("Published ToDoAddedIntegrationEvent for TodoList {ToDoListId}",
            domainEvent.ToDoListId);
    }
}
