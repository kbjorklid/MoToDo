using Base.Application;
using Base.Domain.Result;
using Users.Domain;

namespace Users.Application.Helpers;

/// <summary>
/// Concrete implementation of sort parameter parser for Users domain.
/// </summary>
public static class UsersSortParameterParser
{
    private static readonly SortParameterParser<UsersSortBy> _parser = new(
        fieldMapper: MapFieldToEnum,
        defaultSortBy: UsersSortBy.CreatedAt,
        defaultAscending: true,
        supportedFieldsDescription: "createdAt, email, userName, lastLoginAt",
        errorCodePrefix: "UsersSortParameter"
    );

    /// <summary>
    /// Parses a sort parameter string into UsersSortBy enum and sort direction.
    /// </summary>
    /// <param name="sort">Sort parameter string (e.g., "createdAt", "-email", "userName", "-lastLoginAt").</param>
    /// <returns>A Result containing the parsed sort field and direction, or an error if parsing fails.</returns>
    public static Result<(UsersSortBy SortBy, bool Ascending)> Parse(string? sort)
    {
        return _parser.Parse(sort);
    }

    private static UsersSortBy? MapFieldToEnum(string fieldName)
    {
        return fieldName switch
        {
            "createdat" => UsersSortBy.CreatedAt,
            "email" => UsersSortBy.Email,
            "username" => UsersSortBy.UserName,
            "lastloginat" => UsersSortBy.LastLoginAt,
            _ => null
        };
    }
}
