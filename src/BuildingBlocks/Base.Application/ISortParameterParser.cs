using Base.Domain.Result;

namespace Base.Application;

/// <summary>
/// Generic interface for parsing sort parameters from query strings to domain sorting enums.
/// </summary>
/// <typeparam name="TEnum">The enum type used for sorting fields.</typeparam>
public interface ISortParameterParser<TEnum> where TEnum : struct, Enum
{
    /// <summary>
    /// Parses a sort parameter string into the specified enum type and sort direction.
    /// </summary>
    /// <param name="sort">Sort parameter string (e.g., "fieldName", "-fieldName").</param>
    /// <returns>A Result containing the parsed sort field and direction, or an error if parsing fails.</returns>
    Result<(TEnum SortBy, bool Ascending)> Parse(string? sort);
}
