namespace Base.Domain;

/// <summary>
/// Represents a domain event that occurred within the domain.
/// Domain events are used to communicate changes within and between aggregates.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// The date and time when the domain event occurred.
    /// </summary>
    DateTime OccurredOn { get; }
}
