using AiItemSuggestions.Domain;
using Base.Domain.Result;

namespace AiItemSuggestions.Application.Ports;

/// <summary>
/// Port interface for generating AI-powered todo item suggestions based on existing list content.
/// Provides suggestions that complement the existing items in a todo list.
/// </summary>
public interface IItemSuggestionsPort
{
    /// <summary>
    /// Generates AI-powered suggestions for new todo items based on the existing list content.
    /// </summary>
    /// <param name="snapshot">The immutable snapshot of the todo list to analyze for suggestions.</param>
    /// <param name="count">The number of suggestions to generate (must be between 1 and 20).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A Result containing a list of suggested item titles, or an error if generation fails.</returns>
    Task<Result<IReadOnlyList<SuggestedItemTitle>>> GenerateSuggestionsAsync(
        ToDoListSnapshot snapshot,
        int count,
        CancellationToken cancellationToken = default);
}
