using AiItemSuggestions.Application.Ports;
using AiItemSuggestions.Domain;
using Base.Domain.Result;
using ToDoLists.Contracts;
using Wolverine;

namespace AiItemSuggestions.Infrastructure;

/// <summary>
/// Adapter that fetches ToDoList data from the ToDoLists module via message bus
/// and converts it to ToDoListSnapshot format for AI processing.
/// </summary>
internal sealed class ToDoListDataAdapter : IToDoListDataPort
{
    private readonly IMessageBus _messageBus;

    public ToDoListDataAdapter(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    public async Task<ToDoListSnapshot?> GetToDoListSnapshotAsync(ToDoListId toDoListId, string userId, CancellationToken cancellationToken = default)
    {
        var query = new GetToDoListQuery
        {
            ToDoListId = toDoListId.Value.ToString(),
            UserId = userId
        };

        try
        {
            Result<ToDoListDetailDto> result = await _messageBus.InvokeAsync<Result<ToDoListDetailDto>>(query, cancellationToken);

            if (result.IsFailure)
            {
                return HandleFailureGracefully();
            }

            return ConvertToSnapshot(result.Value);
        }
        catch
        {
            return HandleFailureGracefully();
        }
    }

    private static ToDoListSnapshot? ConvertToSnapshot(ToDoListDetailDto toDoListDetail)
    {
        var toDoListId = new ToDoListId(toDoListDetail.Id);
        List<ToDoItemSnapshot> itemSnapshots = CreateItemSnapshots(toDoListDetail.Todos);

        Result<ToDoListSnapshot> snapshotResult = ToDoListSnapshot.Create(toDoListId, toDoListDetail.Title, itemSnapshots);
        return snapshotResult.IsSuccess ? snapshotResult.Value : null;
    }

    private static List<ToDoItemSnapshot> CreateItemSnapshots(IEnumerable<ToDoDto> todos)
    {
        return todos
            .Select(CreateItemSnapshot)
            .Where(IsSuccessfulSnapshot)
            .Select(ExtractSnapshotValue)
            .ToList();
    }

    private static bool IsSuccessfulSnapshot(Result<ToDoItemSnapshot> result) => result.IsSuccess;

    private static ToDoItemSnapshot ExtractSnapshotValue(Result<ToDoItemSnapshot> result) => result.Value;

    private static Result<ToDoItemSnapshot> CreateItemSnapshot(ToDoDto todo)
    {
        var todoId = new ToDoId(todo.Id);
        return ToDoItemSnapshot.Create(todoId, todo.Title);
    }

    private static ToDoListSnapshot? HandleFailureGracefully() => null;
}
