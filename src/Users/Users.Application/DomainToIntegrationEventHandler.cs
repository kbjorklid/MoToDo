using Users.Contracts;
using Users.Domain;
using Wolverine;

namespace Users.Application;

public class DomainToIntegrationEventHandler
{
    public static async Task Handle(UserDeletedEvent domainEvent, IMessageBus bus)
    {
        UserDeletedIntegrationEvent integrationEvent = new(domainEvent.OccurredOn, domainEvent.UserId.ToString());
        await bus.PublishAsync(integrationEvent);
    }
}
