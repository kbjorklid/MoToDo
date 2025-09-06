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
