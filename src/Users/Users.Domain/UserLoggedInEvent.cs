using Base.Domain;

namespace Users.Domain;

/// <summary>
/// Domain event raised when a user successfully authenticates, enabling activity tracking and security monitoring.
/// </summary>
public sealed record UserLoggedInEvent(UserId UserId, DateTime OccurredOn) : IDomainEvent;
