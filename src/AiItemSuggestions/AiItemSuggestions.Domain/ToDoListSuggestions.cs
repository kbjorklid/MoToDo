using Base.Domain;
using Base.Domain.Result;

namespace AiItemSuggestions.Domain;

/// <summary>
/// The ToDoListSuggestions aggregate root represents AI-powered task recommendations tracking for a specific todo list,
/// maintaining a persistent record of all AI suggestions and their adoption over time.
/// </summary>
public sealed class ToDoListSuggestions : AggregateRoot<ToDoListSuggestionsId>
{
    public static class Codes
    {
        public const string SuggestedItemNotFound = "ToDoListSuggestions.SuggestedItemNotFound";
        public const string ToDoItemNotFound = "ToDoListSuggestions.ToDoItemNotFound";
    }

    private readonly List<SuggestedItem> _suggestedItems = new();

    public ToDoListId ToDoListId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastSuggestionAt { get; private set; }

    /// <summary>
    /// Gets a read-only view of the suggested items in this list.
    /// </summary>
    public IReadOnlyList<SuggestedItem> GetSuggestedItems() => _suggestedItems.AsReadOnly();

    /// <summary>
    /// Gets the number of suggested items in this list.
    /// </summary>
    public int SuggestedItemCount => _suggestedItems.Count;

    private ToDoListSuggestions(ToDoListSuggestionsId id, ToDoListId toDoListId, DateTime createdAt) : base(id)
    {
        ToDoListId = toDoListId;
        CreatedAt = createdAt;
        LastSuggestionAt = null;
    }

    /// <summary>
    /// Creates a new ToDoListSuggestions tracking for the specified todo list.
    /// </summary>
    /// <param name="id">The unique identifier for the suggestions tracking.</param>
    /// <param name="toDoListId">The unique identifier of the todo list being tracked.</param>
    /// <param name="createdAt">The date and time when suggestion tracking was created.</param>
    /// <returns>A new ToDoListSuggestions instance.</returns>
    public static ToDoListSuggestions Create(ToDoListSuggestionsId id, ToDoListId toDoListId, DateTime createdAt)
    {
        return new ToDoListSuggestions(id, toDoListId, createdAt);
    }

    /// <summary>
    /// Adds a new suggested item that corresponds to an actual todo item.
    /// </summary>
    /// <param name="title">The title of the suggested item.</param>
    /// <param name="correspondingToDoId">The ID of the corresponding todo item in the ToDoLists domain.</param>
    /// <param name="addedAt">The date and time when the suggestion was added.</param>
    /// <returns>A Result containing the new SuggestedItem instance if successful, or an error if validation fails.</returns>
    public Result<SuggestedItem> AddSuggestedItem(SuggestedItemTitle title, ToDoId correspondingToDoId, DateTime addedAt)
    {
        var suggestedItemId = SuggestedItemId.New();
        var suggestedItem = SuggestedItem.Create(suggestedItemId, title, correspondingToDoId, addedAt);

        _suggestedItems.Add(suggestedItem);
        LastSuggestionAt = addedAt;

        return suggestedItem;
    }

    /// <summary>
    /// Removes a suggested item from this list.
    /// </summary>
    /// <param name="suggestedItemId">The unique identifier of the suggested item to remove.</param>
    /// <returns>A Result indicating success or failure.</returns>
    public Result RemoveSuggestedItem(SuggestedItemId suggestedItemId)
    {
        SuggestedItem? suggestedItem = FindSuggestedItem(suggestedItemId);
        if (suggestedItem == null)
            return new Error(Codes.SuggestedItemNotFound, "The specified suggested item was not found in this list.", ErrorType.NotFound);

        _suggestedItems.Remove(suggestedItem);

        return Result.Success();
    }

    /// <summary>
    /// Removes a suggested item by its corresponding todo item ID.
    /// </summary>
    /// <param name="toDoId">The unique identifier of the corresponding todo item.</param>
    /// <returns>A Result indicating success or failure.</returns>
    public Result RemoveSuggestedItemByToDoId(ToDoId toDoId)
    {
        SuggestedItem? suggestedItem = FindSuggestedItemByToDoId(toDoId);
        if (suggestedItem == null)
            return new Error(Codes.ToDoItemNotFound, "No suggested item was found for the specified todo item.", ErrorType.NotFound);

        _suggestedItems.Remove(suggestedItem);

        return Result.Success();
    }

    private SuggestedItem? FindSuggestedItem(SuggestedItemId suggestedItemId) =>
        _suggestedItems.FirstOrDefault(s => s.Id == suggestedItemId);

    private SuggestedItem? FindSuggestedItemByToDoId(ToDoId toDoId) =>
        _suggestedItems.FirstOrDefault(s => s.CorrespondingToDoId == toDoId);
}
