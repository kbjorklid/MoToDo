using AiItemSuggestions.Application.Ports;
using AiItemSuggestions.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AiItemSuggestions.Infrastructure;

/// <summary>
/// Extension methods for configuring AiItemSuggestions Infrastructure services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers AiItemSuggestions Infrastructure services with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="connectionString">The database connection string.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddAiItemSuggestionsInfrastructureServices(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<AiItemSuggestionsDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IToDoListSuggestionsRepository, ToDoListSuggestionsRepository>();
        services.AddScoped<IToDoListDataPort, ToDoListDataAdapter>();
        services.AddScoped<IItemSuggestionsService, BogusItemSuggestionsService>();

        return services;
    }
}
