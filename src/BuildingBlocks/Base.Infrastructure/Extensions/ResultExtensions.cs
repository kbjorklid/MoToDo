using Base.Domain.Result;

namespace Base.Infrastructure.Extensions;

/// <summary>
/// Extension methods for Result types to support infrastructure-level operations.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Gets the value from a successful Result or throws InvalidOperationException if the Result is a failure.
    /// This method is intended for scenarios where Result failure represents an unexpected system state
    /// (e.g., database corruption, invalid persisted data) rather than normal business rule violations.
    /// </summary>
    /// <typeparam name="T">The type of the Result value.</typeparam>
    /// <param name="result">The Result to unwrap.</param>
    /// <param name="contextMessage">Optional context message to include in the exception.</param>
    /// <returns>The value if the Result is successful.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the Result is a failure.</exception>
    public static T GetValueOrThrow<T>(this Result<T> result, string? contextMessage = null)
    {
        if (result.IsFailure)
        {
            string message = contextMessage is not null
                ? $"{contextMessage}. Error: {result.Error.Description}"
                : $"Unexpected failure in Result<{typeof(T).Name}>. Error: {result.Error.Description}";
            throw new InvalidOperationException(message);
        }
        return result.Value;
    }
}
