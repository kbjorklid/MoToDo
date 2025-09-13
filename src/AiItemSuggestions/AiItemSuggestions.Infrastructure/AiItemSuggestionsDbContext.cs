using AiItemSuggestions.Domain;
using AiItemSuggestions.Infrastructure.Configurations;
using Base.Domain;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace AiItemSuggestions.Infrastructure;

/// <summary>
/// Database context for the AiItemSuggestions module, configured for PostgreSQL with the 'AiItemSuggestions' schema.
/// Handles domain event publishing through Wolverine message bus integration.
/// </summary>
public sealed class AiItemSuggestionsDbContext : DbContext
{
    private const int NoChanges = 0;
    private readonly IMessageBus? _messageBus;

    public AiItemSuggestionsDbContext(DbContextOptions<AiItemSuggestionsDbContext> options, IMessageBus? messageBus = null) : base(options)
    {
        _messageBus = messageBus;
    }

    public DbSet<ToDoListSuggestions> ToDoListSuggestions => Set<ToDoListSuggestions>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new ToDoListSuggestionsEntityConfiguration());

        // Set default schema for this module as per design plan
        modelBuilder.HasDefaultSchema("AiItemSuggestions");
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        List<IDomainEvent> unpublishedDomainEvents = GetUnpublishedDomainEvents();

        int changesSaved = await base.SaveChangesAsync(cancellationToken);

        await PublishDomainEventsIfSuccessful(changesSaved, unpublishedDomainEvents);

        return changesSaved;
    }

    private List<IDomainEvent> GetUnpublishedDomainEvents()
    {
        return ChangeTracker
            .Entries<AggregateRoot<ToDoListSuggestionsId>>()
            .Where(entry => entry.Entity.GetDomainEvents().Count != 0)
            .SelectMany(entry => entry.Entity.GetDomainEvents())
            .ToList();
    }

    private async Task PublishDomainEventsIfSuccessful(int changesSaved, List<IDomainEvent> domainEvents)
    {
        if (changesSaved == NoChanges || _messageBus is null)
            return;

        await PublishDomainEvents(domainEvents);
        ClearDomainEventsFromAggregateRoots();
    }

    private async Task PublishDomainEvents(List<IDomainEvent> domainEvents)
    {
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            await _messageBus!.PublishAsync(domainEvent);
        }
    }

    private void ClearDomainEventsFromAggregateRoots()
    {
        IEnumerable<AggregateRoot<ToDoListSuggestionsId>> aggregateRoots = ChangeTracker
            .Entries<AggregateRoot<ToDoListSuggestionsId>>()
            .Select(entry => entry.Entity);

        foreach (AggregateRoot<ToDoListSuggestionsId> aggregateRoot in aggregateRoots)
        {
            aggregateRoot.ClearDomainEvents();
        }
    }
}
