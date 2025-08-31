using Base.Domain.Result;
using Users.Domain;

namespace Users.Application.Helpers;

/// <summary>
/// Helper class for parsing sort parameters from query strings to domain sorting enums.
/// </summary>
public static class SortParameterParser
{
    public static class Codes
    {
        public const string UnsupportedSortField = "SortParameterParser.UnsupportedSortField";
    }

    /// <summary>
    /// Parses a sort parameter string into UsersSortBy enum and sort direction.
    /// </summary>
    /// <param name="sort">Sort parameter string (e.g., "createdAt", "-email", "userName", "-lastLoginAt").</param>
    /// <returns>A Result containing the parsed sort field and direction, or an error if parsing fails.</returns>
    public static Result<(UsersSortBy SortBy, bool Ascending)> ParseUsersSortParameter(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return (UsersSortBy.CreatedAt, true);
        }

        bool ascending = true;
        string sortField = sort;

        // Check if sort parameter starts with '-' for descending order
        if (sort.StartsWith('-'))
        {
            ascending = false;
            sortField = sort[1..]; // Remove the '-' prefix
        }

        UsersSortBy? sortByResult = sortField.ToLowerInvariant() switch
        {
            "createdat" => UsersSortBy.CreatedAt,
            "email" => UsersSortBy.Email,
            "username" => UsersSortBy.UserName,
            "lastloginat" => UsersSortBy.LastLoginAt,
            _ => (UsersSortBy?)null
        };

        if (sortByResult is null)
        {
            return new Error(
                Codes.UnsupportedSortField,
                $"Unsupported sort field: '{sortField}'. Supported fields are: createdAt, email, userName, lastLoginAt.",
                ErrorType.Validation
            );
        }

        return (sortByResult.Value, ascending);
    }
}
