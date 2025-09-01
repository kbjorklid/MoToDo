using Base.Domain;
using Base.Domain.Result;
using ToDoLists.Contracts;
using ToDoLists.Domain;

namespace ToDoLists.Application.Queries;

/// <summary>
/// Handles the GetToDoListsQuery to retrieve paginated todo lists.
/// </summary>
public static class GetToDoListsQueryHandler
{
    public static class Codes
    {
        public const string InvalidUserId = "GetToDoLists.InvalidUserId";
        public const string InvalidPagination = "GetToDoLists.InvalidPagination";
    }

    /// <summary>
    /// Handles the query to get paginated todo lists using the repository pattern.
    /// </summary>
    /// <param name="query">The query containing pagination and sorting parameters.</param>
    /// <param name="toDoListRepository">The todo list repository.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <returns>A Result containing GetToDoListsResult if successful, or an error.</returns>
    public static async Task<Result<GetToDoListsResult>> Handle(
        GetToDoListsQuery query,
        IToDoListRepository toDoListRepository,
        CancellationToken cancellationToken)
    {
        // Parse and validate UserId
        Result<UserId> userIdResult = UserId.FromString(query.UserId);
        if (userIdResult.IsFailure)
            return userIdResult.Error;

        // Create pagination parameters
        Result<PagingParameters> pagingParametersResult = PagingParameters.Create(
            query.Page ?? PagingParameters.DefaultPage,
            query.Limit ?? PagingParameters.DefaultLimit
        );

        if (pagingParametersResult.IsFailure)
            return pagingParametersResult.Error;

        // Parse sort parameter
        (ToDoListsSortBy sortBy, bool ascending) = ParseSortParameter(query.Sort);

        // Build query criteria
        Result<ToDoListQueryCriteria> criteriaResult = ToDoListQueryCriteria
            .Builder(pagingParametersResult.Value, userIdResult.Value)
            .WithSortBy(sortBy, ascending)
            .Build();

        if (criteriaResult.IsFailure)
            return criteriaResult.Error;

        // Execute query
        PagedResult<ToDoList> pagedResult = await toDoListRepository.FindToDoListsAsync(criteriaResult.Value, cancellationToken);

        // Map to DTOs
        var summaryDtos = pagedResult.Data
            .Select(tl => new ToDoListSummaryDto(
                tl.Id.Value,
                tl.Title.Value,
                tl.ToDoCount,
                tl.CreatedAt,
                tl.UpdatedAt))
            .ToList();

        var paginationInfo = new PaginationInfo(
            pagedResult.TotalItems,
            pagedResult.TotalPages,
            pagedResult.CurrentPage,
            pagedResult.Limit
        );

        return new GetToDoListsResult(summaryDtos.AsReadOnly(), paginationInfo);
    }

    private static (ToDoListsSortBy sortBy, bool ascending) ParseSortParameter(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
            return (ToDoListsSortBy.CreatedAt, false); // Default to newest first

        bool ascending = !sort.StartsWith('-');
        string fieldName = ascending ? sort : sort[1..];

        ToDoListsSortBy sortBy = fieldName.ToLowerInvariant() switch
        {
            "title" => ToDoListsSortBy.Title,
            "createdat" => ToDoListsSortBy.CreatedAt,
            _ => ToDoListsSortBy.CreatedAt
        };

        return (sortBy, ascending);
    }
}
