using Base.Domain.Result;

namespace Users.Domain;

/// <summary>
/// Strongly typed identifier for User entities that wraps a Guid to provide type safety and prevent primitive obsession.
/// </summary>
public readonly record struct UserId(Guid Value)
{
    public static class Codes
    {
        public const string Empty = "UserId.Empty";
        public const string GuidFormat = "UserId.BadGuidFormat";
    }

    public static UserId New() => new(Guid.NewGuid());

    public static Result<UserId> FromGuid(Guid value)
    {
        return (value == Guid.Empty)
            ? new Error(Codes.Empty, "UserId cannot be empty.", ErrorType.Validation)
            : new UserId(value);
    }

    public static Result<UserId> FromString(string value)
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

    public static implicit operator Guid(UserId userId) => userId.Value;
    public override string ToString() => Value.ToString();
}
