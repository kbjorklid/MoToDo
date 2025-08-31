using System.Net.Mail;
using Base.Domain.Result;

namespace Users.Domain;

/// <summary>
/// Wraps the .NET MailAddress class to provide a domain-specific email representation with validation rules and business logic.
/// </summary>
public readonly record struct Email
{
    public static class Codes
    {
        public const string Empty = "Email.Empty";
        public const string InvalidFormat = "Email.InvalidFormat";
        public const string TooLong = "Email.TooLong";
    }

    public MailAddress Value { get; }

    private Email(MailAddress value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new Email instance with validation.
    /// </summary>
    /// <param name="value">The email address string.</param>
    /// <returns>A Result containing the Email if valid, or an error message if invalid.</returns>
    public static Result<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new Error(Codes.Empty, "Email cannot be null or empty.", ErrorType.Validation);

        if (value.Length > 320)
            return new Error(Codes.TooLong, "Email cannot be longer than 320 characters (RFC 5321 limit).", ErrorType.Validation);

        try
        {
            var mailAddress = new MailAddress(value);
            return new Email(mailAddress);
        }
        catch (FormatException)
        {
            return new Error(Codes.InvalidFormat, $"Invalid email format: {value}", ErrorType.Validation);
        }
    }

    public static implicit operator string(Email email) => email.Value.Address;

    public override string ToString() => Value.Address;
}
