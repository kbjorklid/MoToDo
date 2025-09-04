using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Users.Application.Ports;
using Users.Domain;

namespace Users.Infrastructure;

/// <summary>
/// Extension methods for registering Users Infrastructure services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Users Infrastructure services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="connectionString">The database connection string.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddUsersInfrastructureServices(
        this IServiceCollection services,
        string connectionString)
    {
        // Register DbContext with IMessageBus injection
        services.AddDbContext<UsersDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(connectionString);
        });

        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();

        // Register query context (for CQRS read operations)
        services.AddScoped<IUsersQueryContext>(provider => provider.GetRequiredService<UsersDbContext>());

        return services;
    }
}
