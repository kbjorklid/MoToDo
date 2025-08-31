using Microsoft.EntityFrameworkCore;
using Users.Domain;
using Users.Infrastructure.Configurations;

namespace Users.Infrastructure;

/// <summary>
/// Database context for the Users module, configured for PostgreSQL with the 'Users' schema.
/// </summary>
public sealed class UsersDbContext : DbContext
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
    {
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
}
