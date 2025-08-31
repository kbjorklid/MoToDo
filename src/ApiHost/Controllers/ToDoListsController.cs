using Base.Domain.Result;
using Microsoft.AspNetCore.Mvc;
using ToDoLists.Contracts;
using Wolverine;

namespace ApiHost.Controllers;

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
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)] // TODO: Create proper DTO for get operation
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> GetToDoList(string id)
    {
        // TODO: Implement GetToDoListQuery and handler
        return Task.FromResult<IActionResult>(Ok(new { message = "GetToDoList endpoint not yet implemented" }));
    }

    private IActionResult HandleError(Error error)
    {
        return error.Type switch
        {
            ErrorType.Validation => CreateValidationProblem(error),
            ErrorType.NotFound => NotFound(CreateProblemDetails("Resource not found", error)),
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
            _ => StatusCodes.Status500InternalServerError
        };
    }
}
