using Microsoft.Extensions.DependencyInjection;

namespace AiItemSuggestions.Application;

/// <summary>
/// Extension methods for configuring AiItemSuggestions Application services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers AiItemSuggestions Application services with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddAiItemSuggestionsApplicationServices(this IServiceCollection services)
    {
        // Currently no specific application services to register
        // Command/Query handlers are discovered automatically by Wolverine
        return services;
    }
}
