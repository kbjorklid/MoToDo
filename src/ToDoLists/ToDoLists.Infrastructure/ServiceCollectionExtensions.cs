using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ToDoLists.Domain;

namespace ToDoLists.Infrastructure;

/// <summary>
/// Extension methods for registering ToDoLists Infrastructure services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds ToDoLists Infrastructure services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="connectionString">The database connection string.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddToDoListsInfrastructureServices(
        this IServiceCollection services,
        string connectionString)
    {
        // Register DbContext
        services.AddDbContext<ToDoListsDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Register repositories
        services.AddScoped<IToDoListRepository, ToDoListRepository>();

        return services;
    }
}
