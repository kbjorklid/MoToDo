using Base.Domain.Result;
using Microsoft.AspNetCore.Mvc;
using ToDoLists.Contracts;
using Wolverine;

namespace ApiHost.Controllers;

/// <summary>
/// Request model for adding a new todo item to a list.
/// </summary>
/// <param name="Title">The title of the new todo item.</param>
public sealed record AddToDoRequest(string Title);

/// <summary>
/// Request model for updating a todo item.
/// </summary>
/// <param name="Title">The new title of the todo item (optional).</param>
/// <param name="IsCompleted">The new completion status (optional).</param>
public sealed record UpdateToDoRequest(string? Title, bool? IsCompleted);

/// <summary>
/// REST API controller for todo list management operations.
/// </summary>
[ApiController]
[Route("api/v1/todo-lists")]
[Produces("application/json")]
public class ToDoListsController : ControllerBase
{
    private readonly IMessageBus _messageBus;

    public ToDoListsController(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    /// <summary>
    /// Gets user's todo lists with optional pagination and sorting.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(GetToDoListsResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetToDoLists(
        [FromQuery] string userId,
        [FromQuery] string? sort = null,
        [FromQuery] int? page = null,
        [FromQuery] int? limit = null)
    {
        var query = new GetToDoListsQuery(userId, page, limit, sort);
        Result<GetToDoListsResult> result = await _messageBus.InvokeAsync<Result<GetToDoListsResult>>(query);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return HandleError(result.Error);
    }

    /// <summary>
    /// Creates a new todo list in the system.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateToDoListResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateToDoList([FromBody] CreateToDoListCommand command)
    {
        Result<CreateToDoListResult> result = await _messageBus.InvokeAsync<Result<CreateToDoListResult>>(command);

        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(GetToDoList),
                new { id = result.Value.ToDoListId },
                result.Value);
        }

        return HandleError(result.Error);
    }

    /// <summary>
    /// Retrieves a todo list by its unique identifier.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ToDoListDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetToDoList(string id, [FromQuery] string userId)
    {
        var query = new GetToDoListQuery(id, userId);
        Result<ToDoListDetailDto> result = await _messageBus.InvokeAsync<Result<ToDoListDetailDto>>(query);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return HandleError(result.Error);
    }

    /// <summary>
    /// Adds a new todo item to an existing todo list.
    /// </summary>
    [HttpPost("{listId}/todos")]
    [ProducesResponseType(typeof(AddToDoResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddToDoToList(string listId, [FromBody] AddToDoRequest request)
    {
        var command = new AddToDoCommand(listId, request.Title);
        Result<AddToDoResult> result = await _messageBus.InvokeAsync<Result<AddToDoResult>>(command);

        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(GetToDoList),
                new { id = listId },
                result.Value);
        }

        return HandleError(result.Error);
    }

    /// <summary>
    /// Updates an existing todo item in a specific todo list.
    /// </summary>
    [HttpPut("{listId}/todos/{todoId}")]
    [ProducesResponseType(typeof(UpdateToDoResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateToDo(
        string listId,
        string todoId,
        [FromQuery] string userId,
        [FromBody] UpdateToDoRequest request)
    {
        var command = new UpdateToDoCommand(listId, todoId, userId, request.Title, request.IsCompleted);
        Result<UpdateToDoResult> result = await _messageBus.InvokeAsync<Result<UpdateToDoResult>>(command);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
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
