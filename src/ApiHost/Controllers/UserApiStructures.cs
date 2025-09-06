namespace ApiHost.Controllers;

// API DTOs - Request Models

/// <summary>
/// API request model for adding a new user to the system.
/// </summary>
/// <param name="Email">The user's email address.</param>
/// <param name="UserName">The user's chosen username.</param>
public sealed record AddUserApiRequest(string Email, string UserName);

// API DTOs - Response Models

/// <summary>
/// API response model for adding a new user to the system.
/// </summary>
/// <param name="UserId">The unique identifier of the created user.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="UserName">The user's username.</param>
/// <param name="CreatedAt">The date and time when the user was created.</param>
public sealed record AddUserApiResponse(string UserId, string Email, string UserName, DateTime CreatedAt);

/// <summary>
/// API response model for paginated users.
/// </summary>
/// <param name="Data">The list of user DTOs.</param>
/// <param name="Pagination">Pagination information.</param>
public sealed record GetUsersApiResponse(
    IReadOnlyList<UserApiDto> Data,
    PaginationApiInfo Pagination);

/// <summary>
/// API response model for user information.
/// </summary>
/// <param name="UserId">The unique identifier of the user.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="UserName">The user's username.</param>
/// <param name="CreatedAt">The date and time when the user was created.</param>
/// <param name="LastLoginAt">The date and time when the user last logged in, if applicable.</param>
public sealed record UserApiDto(string UserId, string Email, string UserName, DateTime CreatedAt, DateTime? LastLoginAt);
