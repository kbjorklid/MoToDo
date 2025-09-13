using AiItemSuggestions.Application.Ports;
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
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddAiItemSuggestionsInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IToDoListDataPort, ToDoListDataAdapter>();

        return services;
    }
}
