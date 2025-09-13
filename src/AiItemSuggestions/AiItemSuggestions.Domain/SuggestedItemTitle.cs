using Base.Domain.Result;

namespace AiItemSuggestions.Domain;

/// <summary>
/// Value object representing an AI-generated task suggestion title with validation rules aligned with ToDoLists Title constraints.
/// </summary>
public readonly record struct SuggestedItemTitle(string Value)
{
    public static class Codes
    {
        public const string Empty = "SuggestedItemTitle.Empty";
        public const string TooShort = "SuggestedItemTitle.TooShort";
        public const string TooLong = "SuggestedItemTitle.TooLong";
    }

    public const int MinLength = 3;
    public const int MaxLength = 200;

    public static Result<SuggestedItemTitle> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new Error(Codes.Empty, "Suggested item title cannot be empty or whitespace.", ErrorType.Validation);

        string trimmedValue = value.Trim();

        if (trimmedValue.Length < MinLength)
            return new Error(Codes.TooShort, $"Suggested item title must be at least {MinLength} characters long.", ErrorType.Validation);

        if (trimmedValue.Length > MaxLength)
            return new Error(Codes.TooLong, $"Suggested item title cannot be longer than {MaxLength} characters.", ErrorType.Validation);

        return new SuggestedItemTitle(trimmedValue);
    }

    public static implicit operator string(SuggestedItemTitle title) => title.Value;
    public override string ToString() => Value;
}
