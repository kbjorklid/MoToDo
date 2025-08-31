using Base.Domain;

namespace Users.Domain;

/// <summary>
/// Domain event raised when a user is deleted from the system, enabling cleanup and notification of other system components.
/// </summary>
public sealed record UserDeletedEvent(UserId UserId, Email Email, UserName UserName, DateTime OccurredOn)
    : IDomainEvent;
