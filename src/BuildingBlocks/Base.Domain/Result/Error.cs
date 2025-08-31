namespace Base.Domain.Result;

/// <summary>
/// A structured error type containing a code, description, and type.
/// </summary>
public readonly record struct Error(string Code, string Description, ErrorType Type);
