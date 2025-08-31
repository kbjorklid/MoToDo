using System.Text.RegularExpressions;
using Base.Domain.Result;

namespace Users.Domain;

/// <summary>
/// Represents a user's chosen display name with validation rules appropriate for a professional project planning environment.
/// </summary>
public readonly record struct UserName
{
    public static class Codes
    {
        public const string Empty = "UserName.Empty";
        public const string TooShort = "UserName.TooShort";
        public const string TooLong = "UserName.TooLong";
        public const string InvalidFormat = "UserName.InvalidFormat";
    }

    private static readonly Regex ValidUserNameRegex = new(@"^[a-zA-Z0-9]([a-zA-Z0-9_-]*[a-zA-Z0-9])?$", RegexOptions.Compiled);

    public string Value { get; }

    private UserName(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new UserName instance with validation.
    /// </summary>
    /// <param name="value">The username string.</param>
    /// <returns>A Result containing the UserName if valid, or an error message if invalid.</returns>
    public static Result<UserName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new Error(Codes.Empty, "Username cannot be null or empty.", ErrorType.Validation);

        if (value.Length < 3)
            return new Error(Codes.TooShort, "Username must be at least 3 characters long.", ErrorType.Validation);

        if (value.Length > 50)
            return new Error(Codes.TooLong, "Username cannot be longer than 50 characters.", ErrorType.Validation);

        if (!ValidUserNameRegex.IsMatch(value))
            return new Error(Codes.InvalidFormat, "Username can only contain alphanumeric characters, hyphens, and underscores, and cannot start or end with hyphen or underscore.", ErrorType.Validation);

        return new UserName(value);
    }

    /// <summary>
    /// Implicitly converts a UserName to a string.
    /// </summary>
    /// <param name="userName">The UserName to convert.</param>
    /// <returns>The username as a string.</returns>
    public static implicit operator string(UserName userName) => userName.Value;

    public override string ToString() => Value;
}
