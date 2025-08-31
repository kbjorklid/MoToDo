using Base.Domain;

namespace Users.Domain;

/// <summary>
/// Domain event raised when a new user successfully completes the registration process and is added to the system.
/// </summary>
public sealed record UserRegisteredEvent(UserId UserId, Email Email, UserName UserName, DateTime OccurredOn)
    : IDomainEvent;
