using Base.Contracts;
using Base.Domain.Result;
using Microsoft.AspNetCore.Mvc;
using Users.Contracts;
using Wolverine;

namespace ApiHost.Users;

/// <summary>
/// REST API controller for user management operations.
/// </summary>
[ApiController]
[Route("api/v1/users")]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly IMessageBus _messageBus;

    public UserController(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    /// <summary>
    /// Creates a new user in the system.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AddUserApiResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddUser([FromBody] AddUserApiRequest request)
    {
        AddUserCommand command = new(request.Email, request.UserName);
        Result<AddUserResult> result = await _messageBus.InvokeAsync<Result<AddUserResult>>(command);

        if (result.IsSuccess)
        {
            AddUserApiResponse response = ToApiResponse(result.Value);
            return CreatedAtAction(
                nameof(GetUser),
                new { userId = response.UserId },
                response);
        }

        return HandleError(result.Error);
    }

    /// <summary>
    /// Retrieves a paginated list of users with optional filtering and sorting.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(GetUsersApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int? page = null,
        [FromQuery] int? limit = null,
        [FromQuery] string? sort = null,
        [FromQuery] string? email = null,
        [FromQuery] string? userName = null)
    {
        GetUsersQuery query = new(page, limit, sort, email, userName);
        Result<GetUsersResult> result = await _messageBus.InvokeAsync<Result<GetUsersResult>>(query);

        if (result.IsSuccess)
        {
            GetUsersApiResponse response = ToApiResponse(result.Value);
            return Ok(response);
        }

        return HandleError(result.Error);
    }

    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// </summary>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(UserApiDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUser(string userId)
    {
        GetUserByIdQuery query = new(userId);
        Result<UserDto> result = await _messageBus.InvokeAsync<Result<UserDto>>(query);

        if (result.IsSuccess)
        {
            UserApiDto response = ToApiDto(result.Value);
            return Ok(response);
        }

        return HandleError(result.Error);
    }

    /// <summary>
    /// Deletes a user by their unique identifier.
    /// </summary>
    [HttpDelete("{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        DeleteUserCommand command = new(userId);
        Result result = await _messageBus.InvokeAsync<Result>(command);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return HandleError(result.Error);
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

    private static AddUserApiResponse ToApiResponse(AddUserResult result)
    {
        return new AddUserApiResponse(
            result.UserId.ToString(),
            result.Email,
            result.UserName,
            result.CreatedAt);
    }

    private static GetUsersApiResponse ToApiResponse(GetUsersResult result)
    {
        return new GetUsersApiResponse(
            result.Data.Select(ToApiDto).ToList(),
            ToApiDto(result.Pagination));
    }

    private static UserApiDto ToApiDto(UserDto dto)
    {
        return new UserApiDto(
            dto.UserId.ToString(),
            dto.Email,
            dto.UserName,
            dto.CreatedAt,
            dto.LastLoginAt);
    }

    private static PaginationApiInfo ToApiDto(PaginationInfo pagination)
    {
        return new PaginationApiInfo(
            pagination.TotalItems,
            pagination.TotalPages,
            pagination.CurrentPage,
            pagination.Limit);
    }
}
