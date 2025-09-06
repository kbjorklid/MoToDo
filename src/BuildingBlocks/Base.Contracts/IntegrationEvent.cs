namespace Base.Contracts;

/// <summary>
/// An event sent between modules communicating something that has happened in the source module.
/// </summary>
/// <param name="OccurredOn">The creation time of this event</param>
public abstract record IntegrationEvent(DateTime OccurredOn);
