namespace Base.Domain.Result;

/// <summary>
/// Defines the type of an error.
/// </summary>
public enum ErrorType
{
    Failure,
    Validation,
    NotFound,
    Unexpected
}
