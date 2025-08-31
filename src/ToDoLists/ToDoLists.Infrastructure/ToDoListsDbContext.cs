using Microsoft.EntityFrameworkCore;
using ToDoLists.Domain;
using ToDoLists.Infrastructure.Configurations;

namespace ToDoLists.Infrastructure;

/// <summary>
/// Database context for the ToDoLists module, configured for PostgreSQL with the 'ToDoLists' schema.
/// </summary>
public sealed class ToDoListsDbContext : DbContext
{
    public ToDoListsDbContext(DbContextOptions<ToDoListsDbContext> options) : base(options)
    {
    }

    public DbSet<ToDoList> ToDoLists => Set<ToDoList>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new ToDoListEntityConfiguration());

        // Set default schema for this module as per design plan
        modelBuilder.HasDefaultSchema("ToDoLists");
    }
}
