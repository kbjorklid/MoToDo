using Base.Domain;

namespace AiItemSuggestions.Domain;

/// <summary>
/// An individual AI-generated task suggestion that corresponds to an actual todo item in the ToDoLists domain.
/// </summary>
public sealed class SuggestedItem : Entity<SuggestedItemId>
{
    public SuggestedItemTitle Title { get; private set; }
    public ToDoId CorrespondingToDoId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private SuggestedItem(SuggestedItemId id, SuggestedItemTitle title, ToDoId correspondingToDoId, DateTime createdAt) : base(id)
    {
        Title = title;
        CorrespondingToDoId = correspondingToDoId;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Creates a new SuggestedItem with the specified title and corresponding todo item.
    /// </summary>
    /// <param name="id">The unique identifier for the suggested item.</param>
    /// <param name="title">The title of the suggested item.</param>
    /// <param name="correspondingToDoId">The ID of the corresponding todo item in the ToDoLists domain.</param>
    /// <param name="createdAt">The date and time when the suggestion was created.</param>
    /// <returns>A new SuggestedItem instance.</returns>
    public static SuggestedItem Create(SuggestedItemId id, SuggestedItemTitle title, ToDoId correspondingToDoId, DateTime createdAt)
    {
        return new SuggestedItem(id, title, correspondingToDoId, createdAt);
    }
}
