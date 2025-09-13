using Base.Domain.Result;

namespace AiItemSuggestions.Domain;

/// <summary>
/// Strongly typed identifier for SuggestedItem entities that wraps a Guid to provide type safety and prevent primitive obsession.
/// </summary>
public readonly record struct SuggestedItemId(Guid Value)
{
    public static class Codes
    {
        public const string Empty = "SuggestedItemId.Empty";
        public const string GuidFormat = "SuggestedItemId.GuidFormat";
    }

    public static SuggestedItemId New() => new(Guid.NewGuid());

    public static Result<SuggestedItemId> FromGuid(Guid value)
    {
        return (value == Guid.Empty)
            ? new Error(Codes.Empty, "SuggestedItemId cannot be empty.", ErrorType.Validation)
            : new SuggestedItemId(value);
    }

    public static Result<SuggestedItemId> FromString(string value)
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

    public static implicit operator Guid(SuggestedItemId suggestedItemId) => suggestedItemId.Value;
    public override string ToString() => Value.ToString();
}
