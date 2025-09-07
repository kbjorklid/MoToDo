using Base.Domain.Result;
using ToDoLists.Contracts;
using ToDoLists.Domain;

namespace ToDoLists.Application.Queries;

/// <summary>
/// Handles the GetToDoListQuery to retrieve todo list details.
/// </summary>
public static class GetToDoListQueryHandler
{
    public static class Codes
    {
        public const string NotFound = "GetToDoList.NotFound";
        public const string AccessDenied = "GetToDoList.AccessDenied";
    }

    /// <summary>
    /// Handles the query to get todo list details using the repository pattern.
    /// </summary>
    /// <param name="query">The query containing the todo list ID and user ID.</param>
    /// <param name="toDoListRepository">The todo list repository.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <returns>A Result containing ToDoListDetailDto if successful, or an error.</returns>
    public static async Task<Result<ToDoListDetailDto>> Handle(
        GetToDoListQuery query,
        IToDoListRepository toDoListRepository,
        CancellationToken cancellationToken)
    {
        Result<ToDoListId> toDoListIdResult = ToDoListId.FromString(query.ToDoListId);
        if (toDoListIdResult.IsFailure)
            return toDoListIdResult.Error;

        Result<UserId> userIdResult = UserId.FromString(query.UserId);
        if (userIdResult.IsFailure)
            return userIdResult.Error;

        ToDoList? toDoList = await toDoListRepository.GetByIdAsync(toDoListIdResult.Value, cancellationToken);

        if (toDoList == null)
            return new Error(Codes.NotFound, "Todo list not found.", ErrorType.NotFound);

        if (toDoList.UserId != userIdResult.Value)
            return new Error(Codes.AccessDenied, "Access denied to this todo list.", ErrorType.Forbidden);

        ToDoDto[] todosDto = toDoList.Todos
            .OrderBy(t => t.CreatedAt)
            .Select(t => new ToDoDto(
                t.Id.Value,
                t.Title.Value,
                t.IsCompleted,
                t.CreatedAt,
                t.CompletedAt))
            .ToArray();

        return new ToDoListDetailDto(
            toDoList.Id.Value,
            toDoList.Title.Value,
            todosDto,
            toDoList.ToDoCount,
            toDoList.CreatedAt,
            toDoList.UpdatedAt);
    }
}
