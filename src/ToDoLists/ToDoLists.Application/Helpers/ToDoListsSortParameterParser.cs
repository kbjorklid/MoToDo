using Base.Application;
using Base.Domain.Result;
using ToDoLists.Domain;

namespace ToDoLists.Application.Helpers;

/// <summary>
/// Concrete implementation of sort parameter parser for ToDoLists domain.
/// </summary>
public static class ToDoListsSortParameterParser
{
    private static readonly SortParameterParser<ToDoListsSortBy> _parser = new(
        fieldMapper: MapFieldToEnum,
        defaultSortBy: ToDoListsSortBy.CreatedAt,
        defaultAscending: false, // Default to newest first for todo lists
        supportedFieldsDescription: "title, createdAt",
        errorCodePrefix: "ToDoListsSortParameter"
    );

    /// <summary>
    /// Parses a sort parameter string into ToDoListsSortBy enum and sort direction.
    /// </summary>
    /// <param name="sort">Sort parameter string (e.g., "title", "-createdAt").</param>
    /// <returns>A Result containing the parsed sort field and direction, or an error if parsing fails.</returns>
    public static Result<(ToDoListsSortBy SortBy, bool Ascending)> Parse(string? sort)
    {
        return _parser.Parse(sort);
    }

    private static ToDoListsSortBy? MapFieldToEnum(string fieldName)
    {
        return fieldName switch
        {
            "title" => ToDoListsSortBy.Title,
            "createdat" => ToDoListsSortBy.CreatedAt,
            _ => null
        };
    }
}
