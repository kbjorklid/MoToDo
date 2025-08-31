namespace Base.Domain;

/// <summary>
/// Abstract base class for all aggregate roots in the domain.
/// Provides domain event management capabilities in addition to entity behavior.
/// </summary>
/// <typeparam name="TId">The type of the aggregate root's identifier.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TId}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the aggregate root.</param>
    protected AggregateRoot(TId id) : base(id)
    {
    }

    /// <summary>
    /// Gets the domain events that have been raised by this aggregate root.
    /// </summary>
    /// <returns>A read-only collection of domain events.</returns>
    public IReadOnlyCollection<IDomainEvent> GetDomainEvents()
    {
        return _domainEvents.AsReadOnly();
    }

    /// <summary>
    /// Adds a domain event to be published when the aggregate is persisted.
    /// </summary>
    /// <param name="domainEvent">The domain event to add.</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events from this aggregate root.
    /// This is typically called after domain events have been published.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
