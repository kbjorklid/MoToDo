using Base.Domain.Result;

namespace ToDoLists.Domain;

/// <summary>
/// Value object representing a title for todo lists and todo items with consistent validation rules.
/// </summary>
public readonly record struct Title(string Value)
{
    public static class Codes
    {
        public const string Empty = "Title.Empty";
        public const string TooLong = "Title.TooLong";
    }

    public const int MaxLength = 200;

    public static Result<Title> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new Error(Codes.Empty, "Title cannot be empty or whitespace.", ErrorType.Validation);

        string trimmedValue = value.Trim();
        if (trimmedValue.Length > MaxLength)
            return new Error(Codes.TooLong, $"Title cannot be longer than {MaxLength} characters.", ErrorType.Validation);

        return new Title(trimmedValue);
    }

    public static implicit operator string(Title title) => title.Value;
    public override string ToString() => Value;
}
