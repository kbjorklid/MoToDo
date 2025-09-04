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
        // Get all domain events from tracked aggregate roots BEFORE saving
        var domainEvents = ChangeTracker
            .Entries<AggregateRoot<UserId>>()
            .Where(x => x.Entity.GetDomainEvents().Count != 0)
            .SelectMany(x => x.Entity.GetDomainEvents())
            .ToList();

        // Save changes to the database FIRST
        int result = await base.SaveChangesAsync(cancellationToken);

        // If the save was successful and we have a message bus, dispatch the events
        if (result > 0 && _messageBus is not null)
        {
            foreach (IDomainEvent? domainEvent in domainEvents)
            {
                await _messageBus.PublishAsync(domainEvent);
            }

            // Clear domain events from all aggregate roots after publishing
            IEnumerable<AggregateRoot<UserId>> aggregateRoots = ChangeTracker
                .Entries<AggregateRoot<UserId>>()
                .Select(x => x.Entity);

            foreach (AggregateRoot<UserId>? aggregateRoot in aggregateRoots)
            {
                aggregateRoot.ClearDomainEvents();
            }
        }

        return result;
    }
}
