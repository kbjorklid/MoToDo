using Base.Domain.Result;

namespace AiItemSuggestions.Domain;

/// <summary>
/// Strongly typed identifier for ToDoListSuggestions aggregates that wraps a Guid to provide type safety and prevent primitive obsession.
/// </summary>
public readonly record struct ToDoListSuggestionsId(Guid Value)
{
    public static class Codes
    {
        public const string Empty = "ToDoListSuggestionsId.Empty";
        public const string GuidFormat = "ToDoListSuggestionsId.GuidFormat";
    }

    public static ToDoListSuggestionsId New() => new(Guid.NewGuid());

    public static Result<ToDoListSuggestionsId> FromGuid(Guid value)
    {
        return (value == Guid.Empty)
            ? new Error(Codes.Empty, "ToDoListSuggestionsId cannot be empty.", ErrorType.Validation)
            : new ToDoListSuggestionsId(value);
    }

    public static Result<ToDoListSuggestionsId> FromString(string value)
    {
        try
        {
            return FromGuid(Guid.Parse(value));
        }
        catch (FormatException)
        {
            return new Error(Codes.GuidFormat, $"Invalid Guid format: {value}.", ErrorType.Validation);
        }
    }

    public static implicit operator Guid(ToDoListSuggestionsId toDoListSuggestionsId) => toDoListSuggestionsId.Value;
    public override string ToString() => Value.ToString();
}
