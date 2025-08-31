using Base.Domain.Result;

namespace ToDoLists.Domain;

/// <summary>
/// Strongly typed identifier for ToDo entities that wraps a Guid to provide type safety and prevent primitive obsession.
/// </summary>
public readonly record struct ToDoId(Guid Value)
{
    public static class Codes
    {
        public const string Empty = "ToDoId.Empty";
        public const string GuidFormat = "ToDoId.BadGuidFormat";
    }

    public static ToDoId New() => new(Guid.NewGuid());

    public static Result<ToDoId> FromGuid(Guid value)
    {
        return (value == Guid.Empty)
            ? new Error(Codes.Empty, "ToDoId cannot be empty.", ErrorType.Validation)
            : new ToDoId(value);
    }

    public static Result<ToDoId> FromString(string value)
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

    public static implicit operator Guid(ToDoId toDoId) => toDoId.Value;
    public override string ToString() => Value.ToString();
}
