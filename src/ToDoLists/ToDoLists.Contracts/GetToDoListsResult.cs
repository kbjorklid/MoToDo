namespace ToDoLists.Contracts;

/// <summary>
/// Result of successfully retrieving todo lists with pagination information.
/// </summary>
/// <param name="Data">The list of todo list summary DTOs returned by the query.</param>
/// <param name="Pagination">Pagination information including totals and current page.</param>
public sealed record GetToDoListsResult(
    IReadOnlyList<ToDoListSummaryDto> Data,
    PaginationInfo Pagination);
