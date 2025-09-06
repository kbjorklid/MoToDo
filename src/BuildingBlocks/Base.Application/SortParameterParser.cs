using Base.Domain.Result;

namespace Base.Application;

/// <summary>
/// Generic implementation for parsing sort parameters from query strings to domain sorting enums.
/// </summary>
/// <typeparam name="TEnum">The enum type used for sorting fields.</typeparam>
public class SortParameterParser<TEnum> : ISortParameterParser<TEnum> where TEnum : struct, Enum
{
    private readonly Func<string, TEnum?> _fieldMapper;
    private readonly TEnum _defaultSortBy;
    private readonly bool _defaultAscending;
    private readonly string _supportedFieldsDescription;
    private readonly string _errorCodePrefix;

    /// <summary>
    /// Initializes a new instance of the SortParameterParser.
    /// </summary>
    /// <param name="fieldMapper">Function that maps field names to enum values. Should return null for unsupported fields.</param>
    /// <param name="defaultSortBy">Default sort field when no sort parameter is provided.</param>
    /// <param name="defaultAscending">Default sort direction when no sort parameter is provided.</param>
    /// <param name="supportedFieldsDescription">Description of supported fields for error messages.</param>
    /// <param name="errorCodePrefix">Prefix for error codes (e.g., "UsersSortParameter").</param>
    public SortParameterParser(
        Func<string, TEnum?> fieldMapper,
        TEnum defaultSortBy,
        bool defaultAscending = true,
        string supportedFieldsDescription = "",
        string errorCodePrefix = "SortParameter")
    {
        _fieldMapper = fieldMapper ?? throw new ArgumentNullException(nameof(fieldMapper));
        _defaultSortBy = defaultSortBy;
        _defaultAscending = defaultAscending;
        _supportedFieldsDescription = supportedFieldsDescription;
        _errorCodePrefix = errorCodePrefix;
    }

    /// <summary>
    /// Parses a sort parameter string into the specified enum type and sort direction.
    /// </summary>
    /// <param name="sort">Sort parameter string (e.g., "fieldName", "-fieldName").</param>
    /// <returns>A Result containing the parsed sort field and direction, or an error if parsing fails.</returns>
    public Result<(TEnum SortBy, bool Ascending)> Parse(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return (_defaultSortBy, _defaultAscending);
        }

        bool ascending = true;
        string sortField = sort;

        // Check if sort parameter starts with '-' for descending order
        if (sort.StartsWith('-'))
        {
            ascending = false;
            sortField = sort[1..]; // Remove the '-' prefix
        }

        TEnum? sortByResult = _fieldMapper(sortField.ToLowerInvariant());

        if (sortByResult is not null)
            return (sortByResult.Value, ascending);

        return new Error(
            $"{_errorCodePrefix}.UnsupportedField",
            $"Unsupported sort field: '{sortField}'. Supported fields: {_supportedFieldsDescription}.",
            ErrorType.Validation);

    }
}
