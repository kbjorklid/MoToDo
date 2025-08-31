# ApiHost Project Conventions

This document provides specific implementation conventions for the `ApiHost` project, focusing on controller patterns 
and Wolverine message bus integration. Assumes familiarity with ARCHITECTURE.md, CODING_CONVENTIONS.md, and 
REST_CONVENTIONS.md.

## Controller Implementation

### Basic Structure

Controllers act as thin orchestrators that translate HTTP requests into commands/queries and relay responses:

```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class YourController : ControllerBase
{
    private readonly IMessageBus _messageBus;

    public YourController(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }
}
```

### Action Method Patterns

#### Command Handling (Write Operations)
```csharp
[HttpPost]
public async Task<IActionResult> CreateResource([FromBody] CreateResourceCommand command)
{
    Result<CreateResourceResult> result = await _messageBus.InvokeAsync<Result<CreateResourceResult>>(command);

    if (result.IsSuccess)
    {
        return CreatedAtAction(
            nameof(GetResource),
            new { resourceId = result.Value.ResourceId },
            result.Value);
    }

    return HandleError(result.Error);
}
```

#### Query Handling (Read Operations)
```csharp
[HttpGet("{resourceId}")]
public async Task<IActionResult> GetResource(string resourceId)
{
    var query = new GetResourceByIdQuery(resourceId);
    Result<ResourceDto> result = await _messageBus.InvokeAsync<Result<ResourceDto>>(query);

    if (result.IsSuccess)
    {
        var response = new { data = result.Value };
        return Ok(response);
    }

    return HandleError(result.Error);
}
```

### Wolverine Message Bus Usage

#### Key Methods
- `InvokeAsync<T>(message)`: Request/reply pattern with expected response type
- `InvokeAsync(message)`: Fire-and-forget command execution
- `PublishAsync(message)`: Event broadcasting (use for domain events)
- `SendAsync(message)`: Guaranteed delivery to specific handler

#### Response Wrapping
Always wrap raw array responses in `data` envelope per REST conventions:
```csharp
var response = new { data = someArrayOfDtos };
return Ok(response);
```

For single objects, return the object directly (no need to wrap with another object to use the `data` key).

### Error Handling Pattern

Use consistent error handling across all actions:

```csharp
private IActionResult HandleError(Error error)
{
    return error.Type switch
    {
        ErrorType.Validation => ValidationProblem(error),
        ErrorType.NotFound => NotFound(CreateProblemDetails("Resource not found", error)),
        _ => Problem(
            title: "An error occurred while processing the request",
            detail: error.Description,
            statusCode: StatusCodes.Status500InternalServerError)
    };
}

private ValidationProblemDetails ValidationProblem(Error error)
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
```

### ProducesResponseType Attributes

Document all possible response types:

```csharp
[ProducesResponseType(typeof(ResourceDto), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
```

## Dependency Injection

### Service Registration
Register controllers using standard ASP.NET Core conventions in `Program.cs`:

```csharp
builder.Services.AddControllers();
```

Wolverine's `IMessageBus` is automatically registered when using `.UseWolverine()`.

## File Organization

- Controllers in `Controllers/` folder
- One controller per aggregate/bounded context
- Controller names match module names (e.g., `UsersController` for Users module)
- Use `ControllerBase` instead of `Controller` (no view support needed)

## Best Practices

1. **Keep controllers thin** - No business logic, only HTTP concerns
2. **Use Result pattern** - Always handle `Result<T>` from message handlers
3. **Consistent error responses** - Use standardized error handling helper methods
4. **Proper status codes** - Follow REST conventions for HTTP status codes
5. **XML documentation** - Document controller purpose and action method parameters/returns
6. **Route consistency** - Use versioned routes (`api/v1/`) and kebab-case naming