using Base.Contracts;

namespace Users.Contracts;

public record UserDeletedIntegrationEvent(DateTime OccurredOn, string UserId) : IntegrationEvent(OccurredOn);
