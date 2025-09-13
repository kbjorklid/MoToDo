using ToDoLists.Contracts;
using ToDoLists.Domain;
using Wolverine;

namespace ToDoLists.Application;

/// <summary>
/// Contains handlers that convert domain events to integration events for inter-module communication.
/// </summary>
public class DomainToIntegrationEventHandlers
{
    public static async Task Handle(ToDoAddedEvent domainEvent, IMessageBus bus)
    {
        ToDoAddedIntegrationEvent integrationEvent = new(
            domainEvent.OccurredOn,
            domainEvent.ToDoListId.ToString(),
            domainEvent.ToDoId.ToString(),
            domainEvent.UserId.ToString());

        await bus.PublishAsync(integrationEvent);
    }
}
