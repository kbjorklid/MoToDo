using AiItemSuggestions.Application.Ports;
using AiItemSuggestions.Domain;
using Base.Domain.Result;

namespace AiItemSuggestions.Infrastructure;

/// <summary>
/// Bogus implementation of the item suggestions service that provides hardcoded suggestions
/// without actually using an LLM. This is useful for development, testing, and demonstration purposes.
/// </summary>
internal sealed class BogusItemSuggestionsService : IItemSuggestionsService
{
    public static class Codes
    {
        public const string InvalidCount = "BogusItemSuggestionsService.InvalidCount";
        public const string NullSnapshot = "BogusItemSuggestionsService.NullSnapshot";
    }

    private static class Constants
    {
        public const int MinSimulatedDelayMs = 100;
        public const int MaxSimulatedDelayMs = 500;
        public const int MaxAttempts = 50;
        public const string FallbackSuggestionPrefix = "Task suggestion";
    }

    private static readonly string[] SuggestionTemplates =
    {
        "Review and prioritize tasks",
        "Schedule follow-up meeting",
        "Update project documentation",
        "Research best practices",
        "Prepare status report",
        "Clean up workspace",
        "Back up important files",
        "Plan next sprint activities",
        "Test implementation thoroughly",
        "Gather user feedback",
        "Optimize performance metrics",
        "Create deployment checklist",
        "Set up monitoring alerts",
        "Update team on progress",
        "Archive completed items",
        "Refactor legacy code",
        "Write unit tests",
        "Design user interface",
        "Configure security settings",
        "Validate data integrity",
        "Schedule code review",
        "Document API endpoints",
        "Plan rollback strategy",
        "Coordinate with stakeholders",
        "Analyze usage patterns"
    };

    public async Task<Result<IReadOnlyList<SuggestedItemTitle>>> GenerateSuggestionsAsync(
        ToDoListSnapshot snapshot,
        int count,
        CancellationToken cancellationToken = default)
    {
        if (snapshot == null)
            return new Error(Codes.NullSnapshot, "Todo list snapshot cannot be null.", ErrorType.Validation);

        if (count < 1 || count > 20)
            return new Error(Codes.InvalidCount, "Suggestion count must be between 1 and 20.", ErrorType.Validation);

        // Simulate AI processing time
        await Task.Delay(Random.Shared.Next(Constants.MinSimulatedDelayMs, Constants.MaxSimulatedDelayMs), cancellationToken);

        var suggestions = new List<SuggestedItemTitle>();
        var random = new Random();
        var usedIndices = new HashSet<int>();

        // Get existing item titles to avoid duplicates (case-insensitive)
        var existingTitles = new HashSet<string>(
            snapshot.Items.Select(item => item.Title),
            StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < count; i++)
        {
            string suggestion = SelectUniqueSuggestion(random, usedIndices, existingTitles, i);
            string finalSuggestion = EnsureUniqueSuggestion(suggestion, existingTitles, i);

            Result<SuggestedItemTitle> titleResult = SuggestedItemTitle.Create(finalSuggestion);
            if (titleResult.IsSuccess)
            {
                suggestions.Add(titleResult.Value);
                existingTitles.Add(finalSuggestion);
            }
        }

        return suggestions.AsReadOnly();
    }

    private static string EnsureUniqueSuggestion(string suggestion, HashSet<string> existingTitles, int suggestionNumber)
    {
        return existingTitles.Contains(suggestion)
            ? $"{suggestion} ({suggestionNumber + 1})"
            : suggestion;
    }

    private static string SelectUniqueSuggestion(Random random, HashSet<int> usedIndices, HashSet<string> existingTitles, int suggestionNumber)
    {
        string suggestion;
        int attempts = 0;

        do
        {
            int index;
            do
            {
                index = random.Next(SuggestionTemplates.Length);
                attempts++;
            } while (usedIndices.Contains(index) && attempts < Constants.MaxAttempts);

            if (attempts >= Constants.MaxAttempts)
            {
                return $"{Constants.FallbackSuggestionPrefix} {suggestionNumber + 1}";
            }

            usedIndices.Add(index);
            suggestion = SuggestionTemplates[index];
            attempts++;
        } while (existingTitles.Contains(suggestion) && attempts < Constants.MaxAttempts);

        return suggestion;
    }
}
