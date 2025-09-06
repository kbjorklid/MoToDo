using Base.Application;
using Base.Contracts;
using Base.Domain;
using Base.Domain.Result;
using ToDoLists.Application.Helpers;
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
        Result<UserId> userIdResult = UserId.FromString(query.UserId);
        if (userIdResult.IsFailure)
            return userIdResult.Error;

        Result<PagingParameters> pagingParametersResult = PaginationHelpers.CreatePagingParameters(query.Page, query.Limit);
        if (pagingParametersResult.IsFailure)
            return pagingParametersResult.Error;

        Result<(ToDoListsSortBy sortBy, bool ascending)> sortResult = ToDoListsSortParameterParser.Parse(query.Sort);
        if (sortResult.IsFailure)
            return sortResult.Error;

        Result<ToDoListQueryCriteria> criteriaResult = ToDoListQueryCriteria
            .Builder(pagingParametersResult.Value, userIdResult.Value)
            .WithSortBy(sortResult.Value.sortBy, sortResult.Value.ascending)
            .Build();

        if (criteriaResult.IsFailure)
            return criteriaResult.Error;

        PagedResult<ToDoList> pagedResult = await toDoListRepository.FindToDoListsAsync(criteriaResult.Value, cancellationToken);

        var summaryDtos = pagedResult.Data
            .Select(tl => new ToDoListSummaryDto(
                tl.Id.Value,
                tl.Title.Value,
                tl.ToDoCount,
                tl.CreatedAt,
                tl.UpdatedAt))
            .ToList();

        PaginationInfo paginationInfo = PaginationHelpers.CreatePaginationInfo(pagedResult);

        return new GetToDoListsResult(summaryDtos.AsReadOnly(), paginationInfo);
    }

}
