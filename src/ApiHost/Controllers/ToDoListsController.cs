using Base.Contracts;
using Base.Domain.Result;
using Microsoft.AspNetCore.Mvc;
using ToDoLists.Contracts;
using Wolverine;

namespace ApiHost.Controllers;

// API DTOs - Request Models

/// <summary>
/// API request model for creating a new todo list.
/// </summary>
/// <param name="UserId">The unique identifier of the user who owns the list.</param>
/// <param name="Title">The title of the todo list.</param>
public sealed record CreateToDoListApiRequest(string UserId, string Title);

/// <summary>
/// API request model for adding a new todo item to a list.
/// </summary>
/// <param name="Title">The title of the new todo item.</param>
public sealed record AddToDoApiRequest(string Title);

/// <summary>
/// API request model for updating a todo item.
/// </summary>
/// <param name="Title">The new title of the todo item (optional).</param>
/// <param name="IsCompleted">The new completion status (optional).</param>
public sealed record UpdateToDoApiRequest(string? Title, bool? IsCompleted);

/// <summary>
/// API request model for updating todo list title.
/// </summary>
/// <param name="Title">The new title for the todo list.</param>
public sealed record UpdateToDoListTitleApiRequest(string Title);

// API DTOs - Response Models

/// <summary>
/// API response model for creating a new todo list.
/// </summary>
/// <param name="Id">The unique identifier of the created todo list.</param>
/// <param name="UserId">The unique identifier of the user who owns the list.</param>
/// <param name="Title">The title of the todo list.</param>
/// <param name="CreatedAt">The date and time when the todo list was created.</param>
public sealed record CreateToDoListApiResponse(string Id, string UserId, string Title, DateTime CreatedAt);

/// <summary>
/// API response model for paginated todo lists.
/// </summary>
/// <param name="Data">The list of todo list summaries.</param>
/// <param name="Pagination">Pagination information.</param>
public sealed record GetToDoListsApiResponse(
    IReadOnlyList<ToDoListSummaryApiDto> Data,
    PaginationApiInfo Pagination);

/// <summary>
/// API response model for todo list summary information.
/// </summary>
/// <param name="Id">The unique identifier of the todo list.</param>
/// <param name="Title">The title of the todo list.</param>
/// <param name="TodoCount">Number of todos in the list.</param>
/// <param name="CreatedAt">When the todo list was created.</param>
/// <param name="UpdatedAt">When the todo list was last updated.</param>
public sealed record ToDoListSummaryApiDto(
    string Id,
    string Title,
    int TodoCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>
/// API response model for detailed todo list information.
/// </summary>
/// <param name="Id">The unique identifier of the todo list.</param>
/// <param name="Title">The title of the todo list.</param>
/// <param name="Todos">Array of todos in the list.</param>
/// <param name="TodoCount">Number of todos in the list.</param>
/// <param name="CreatedAt">When the todo list was created.</param>
/// <param name="UpdatedAt">When the todo list was last updated.</param>
public sealed record ToDoListDetailApiResponse(
    string Id,
    string Title,
    ToDoApiDto[] Todos,
    int TodoCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>
/// API response model for todo item information.
/// </summary>
/// <param name="Id">The unique identifier of the todo item.</param>
/// <param name="Title">The title of the todo item.</param>
/// <param name="IsCompleted">Whether the todo item is completed.</param>
/// <param name="CreatedAt">When the todo item was created.</param>
/// <param name="CompletedAt">When the todo item was completed, if applicable.</param>
public sealed record ToDoApiDto(
    string Id,
    string Title,
    bool IsCompleted,
    DateTime CreatedAt,
    DateTime? CompletedAt);

/// <summary>
/// API response model for adding a new todo item.
/// </summary>
/// <param name="Id">The unique identifier of the created todo item.</param>
/// <param name="Title">The title of the todo item.</param>
/// <param name="IsCompleted">Whether the todo item is completed.</param>
/// <param name="CreatedAt">The date and time when the todo item was created.</param>
/// <param name="CompletedAt">The date and time when the todo item was completed, if applicable.</param>
public sealed record AddToDoApiResponse(string Id, string Title, bool IsCompleted, DateTime CreatedAt, DateTime? CompletedAt);

/// <summary>
/// API response model for updating a todo item.
/// </summary>
/// <param name="Id">The unique identifier of the updated todo item.</param>
/// <param name="Title">The title of the todo item.</param>
/// <param name="IsCompleted">Whether the todo item is completed.</param>
/// <param name="CreatedAt">The date and time when the todo item was created.</param>
/// <param name="CompletedAt">The date and time when the todo item was completed, if applicable.</param>
public sealed record UpdateToDoApiResponse(string Id, string Title, bool IsCompleted, DateTime CreatedAt, DateTime? CompletedAt);

/// <summary>
/// API pagination information.
/// </summary>
/// <param name="TotalItems">Total number of items available.</param>
/// <param name="TotalPages">Total number of pages available.</param>
/// <param name="CurrentPage">Current page number.</param>
/// <param name="Limit">Number of items per page.</param>
public sealed record PaginationApiInfo(
    int TotalItems,
    int TotalPages,
    int CurrentPage,
    int Limit);

/// <summary>
/// REST API controller for todo list management operations.
/// </summary>
[ApiController]
[Route("api/v1/todo-lists")]
[Produces("application/json")]
public class ToDoListsController : ControllerBase
{

    // Mapping Methods - Contracts to API DTOs

    private static CreateToDoListApiResponse ToApiResponse(CreateToDoListResult result)
    {
        return new CreateToDoListApiResponse(
            result.ToDoListId.ToString(),
            result.UserId.ToString(),
            result.Title,
            result.CreatedAt);
    }

    private static GetToDoListsApiResponse ToApiResponse(GetToDoListsResult result)
    {
        return new GetToDoListsApiResponse(
            result.Data.Select(ToApiDto).ToList(),
            ToApiDto(result.Pagination));
    }

    private static ToDoListSummaryApiDto ToApiDto(ToDoListSummaryDto dto)
    {
        return new ToDoListSummaryApiDto(
            dto.Id.ToString(),
            dto.Title,
            dto.TodoCount,
            dto.CreatedAt,
            dto.UpdatedAt);
    }

    private static ToDoListDetailApiResponse ToApiResponse(ToDoListDetailDto dto)
    {
        return new ToDoListDetailApiResponse(
            dto.Id.ToString(),
            dto.Title,
            dto.Todos.Select(ToApiDto).ToArray(),
            dto.TodoCount,
            dto.CreatedAt,
            dto.UpdatedAt);
    }

    private static ToDoApiDto ToApiDto(ToDoDto dto)
    {
        return new ToDoApiDto(
            dto.Id.ToString(),
            dto.Title,
            dto.IsCompleted,
            dto.CreatedAt,
            dto.CompletedAt);
    }

    private static AddToDoApiResponse ToApiResponse(AddToDoResult result)
    {
        return new AddToDoApiResponse(
            result.Id.ToString(),
            result.Title,
            result.IsCompleted,
            result.CreatedAt,
            result.CompletedAt);
    }

    private static UpdateToDoApiResponse ToApiResponse(UpdateToDoResult result)
    {
        return new UpdateToDoApiResponse(
            result.Id.ToString(),
            result.Title,
            result.IsCompleted,
            result.CreatedAt,
            result.CompletedAt);
    }

    private static PaginationApiInfo ToApiDto(PaginationInfo pagination)
    {
        return new PaginationApiInfo(
            pagination.TotalItems,
            pagination.TotalPages,
            pagination.CurrentPage,
            pagination.Limit);
    }
    private readonly IMessageBus _messageBus;

    public ToDoListsController(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    /// <summary>
    /// Gets user's todo lists with optional pagination and sorting.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(GetToDoListsApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetToDoLists(
        [FromQuery] string userId,
        [FromQuery] string? sort = null,
        [FromQuery] int? page = null,
        [FromQuery] int? limit = null)
    {
        GetToDoListsQuery query = new(userId, page, limit, sort);
        Result<GetToDoListsResult> result = await _messageBus.InvokeAsync<Result<GetToDoListsResult>>(query);

        if (result.IsSuccess)
        {
            GetToDoListsApiResponse response = ToApiResponse(result.Value);
            return Ok(response);
        }

        return HandleError(result.Error);
    }

    /// <summary>
    /// Creates a new todo list in the system.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateToDoListApiResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateToDoList([FromBody] CreateToDoListApiRequest request)
    {
        CreateToDoListCommand command = new(request.UserId, request.Title);
        Result<CreateToDoListResult> result = await _messageBus.InvokeAsync<Result<CreateToDoListResult>>(command);

        if (result.IsSuccess)
        {
            CreateToDoListApiResponse response = ToApiResponse(result.Value);
            return CreatedAtAction(
                nameof(GetToDoList),
                new { id = response.Id },
                response);
        }

        return HandleError(result.Error);
    }

    /// <summary>
    /// Retrieves a todo list by its unique identifier.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ToDoListDetailApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetToDoList(string id, [FromQuery] string userId)
    {
        GetToDoListQuery query = new(id, userId);
        Result<ToDoListDetailDto> result = await _messageBus.InvokeAsync<Result<ToDoListDetailDto>>(query);

        if (result.IsSuccess)
        {
            ToDoListDetailApiResponse response = ToApiResponse(result.Value);
            return Ok(response);
        }

        return HandleError(result.Error);
    }

    /// <summary>
    /// Updates the title of an existing todo list.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ToDoListDetailApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateToDoListTitle(
        string id,
        [FromQuery] string userId,
        [FromBody] UpdateToDoListTitleApiRequest request)
    {
        UpdateToDoListTitleCommand command = new(id, userId, request.Title);
        Result<UpdateToDoListTitleResult> result = await _messageBus.InvokeAsync<Result<UpdateToDoListTitleResult>>(command);

        if (result.IsSuccess)
        {
            // Return the complete ToDoListDetailApiResponse by fetching the updated list
            GetToDoListQuery getQuery = new(id, userId);
            Result<ToDoListDetailDto> detailResult = await _messageBus.InvokeAsync<Result<ToDoListDetailDto>>(getQuery);

            if (detailResult.IsSuccess)
            {
                ToDoListDetailApiResponse response = ToApiResponse(detailResult.Value);
                return Ok(response);
            }

            return HandleError(detailResult.Error);
        }

        return HandleError(result.Error);
    }

    /// <summary>
    /// Adds a new todo item to an existing todo list.
    /// </summary>
    [HttpPost("{listId}/todos")]
    [ProducesResponseType(typeof(AddToDoApiResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddToDoToList(string listId, [FromBody] AddToDoApiRequest request)
    {
        AddToDoCommand command = new(listId, request.Title);
        Result<AddToDoResult> result = await _messageBus.InvokeAsync<Result<AddToDoResult>>(command);

        if (result.IsSuccess)
        {
            AddToDoApiResponse response = ToApiResponse(result.Value);
            return CreatedAtAction(
                nameof(GetToDoList),
                new { id = listId },
                response);
        }

        return HandleError(result.Error);
    }

    /// <summary>
    /// Updates an existing todo item in a specific todo list.
    /// </summary>
    [HttpPut("{listId}/todos/{todoId}")]
    [ProducesResponseType(typeof(UpdateToDoApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateToDo(
        string listId,
        string todoId,
        [FromQuery] string userId,
        [FromBody] UpdateToDoApiRequest request)
    {
        UpdateToDoCommand command = new(listId, todoId, userId, request.Title, request.IsCompleted);
        Result<UpdateToDoResult> result = await _messageBus.InvokeAsync<Result<UpdateToDoResult>>(command);

        if (result.IsSuccess)
        {
            UpdateToDoApiResponse response = ToApiResponse(result.Value);
            return Ok(response);
        }

        return HandleError(result.Error);
    }

    private IActionResult HandleError(Error error)
    {
        return error.Type switch
        {
            ErrorType.Validation => CreateValidationProblem(error),
            ErrorType.NotFound => NotFound(CreateProblemDetails("Resource not found", error)),
            ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, CreateProblemDetails("Forbidden", error)),
            _ => Problem(
                title: "An error occurred while processing the request",
                detail: error.Description,
                statusCode: StatusCodes.Status500InternalServerError)
        };
    }

    private IActionResult CreateValidationProblem(Error error)
    {
        ModelState.AddModelError(string.Empty, error.Description);
        return ValidationProblem();
    }

    private ProblemDetails CreateProblemDetails(string title, Error error)
    {
        return new ProblemDetails
        {
            Title = title,
            Detail = error.Description,
            Status = GetStatusCodeForErrorType(error.Type)
        };
    }

    private static int GetStatusCodeForErrorType(ErrorType errorType)
    {
        return errorType switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}
