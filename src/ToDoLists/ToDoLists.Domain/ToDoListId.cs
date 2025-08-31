using Base.Domain.Result;

namespace ToDoLists.Domain;

/// <summary>
/// Strongly typed identifier for ToDoList entities that wraps a Guid to provide type safety and prevent primitive obsession.
/// </summary>
public readonly record struct ToDoListId(Guid Value)
{
    public static class Codes
    {
        public const string Empty = "ToDoListId.Empty";
        public const string GuidFormat = "ToDoListId.BadGuidFormat";
    }

    public static ToDoListId New() => new(Guid.NewGuid());

    public static Result<ToDoListId> FromGuid(Guid value)
    {
        return (value == Guid.Empty)
            ? new Error(Codes.Empty, "ToDoListId cannot be empty.", ErrorType.Validation)
            : new ToDoListId(value);
    }

    public static Result<ToDoListId> FromString(string value)
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

    public static implicit operator Guid(ToDoListId toDoListId) => toDoListId.Value;
    public override string ToString() => Value.ToString();
}
