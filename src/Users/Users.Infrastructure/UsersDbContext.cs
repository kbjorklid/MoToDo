using Base.Domain;
using Microsoft.EntityFrameworkCore;
using Users.Application.Ports;
using Users.Domain;
using Users.Infrastructure.Configurations;
using Wolverine;

namespace Users.Infrastructure;

/// <summary>
/// Database context for the Users module, configured for PostgreSQL with the 'Users' schema.
/// Supports automatic domain event publishing through Wolverine message bus.
/// </summary>
public sealed class UsersDbContext : DbContext, IUsersQueryContext
{
    private readonly IMessageBus? _messageBus;

    public UsersDbContext(DbContextOptions<UsersDbContext> options, IMessageBus? messageBus = null) : base(options)
    {
        _messageBus = messageBus;
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new UserEntityConfiguration());

        // Set default schema for this module as per design plan
        modelBuilder.HasDefaultSchema("Users");
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
            .Entries<AggregateRoot<UserId>>()
            .Where(entry => entry.Entity.GetDomainEvents().Count != 0)
            .SelectMany(entry => entry.Entity.GetDomainEvents())
            .ToList();
    }

    private async Task PublishDomainEventsIfSuccessful(int changesSaved, List<IDomainEvent> domainEvents)
    {
        const int NoChanges = 0;

        if (changesSaved == NoChanges || _messageBus is null)
            return;

        await PublishDomainEvents(domainEvents);
        ClearDomainEventsFromAggregateRoots();
    }

    private async Task PublishDomainEvents(List<IDomainEvent> domainEvents)
    {
        foreach (IDomainEvent domainEvent in domainEvents) 
            await _messageBus!.PublishAsync(domainEvent);
    }

    private void ClearDomainEventsFromAggregateRoots()
    {
        IEnumerable<AggregateRoot<UserId>> aggregateRootsWithEvents = ChangeTracker
            .Entries<AggregateRoot<UserId>>()
            .Select(entry => entry.Entity);

        foreach (AggregateRoot<UserId> aggregateRoot in aggregateRootsWithEvents) 
            aggregateRoot.ClearDomainEvents();
    }
}
