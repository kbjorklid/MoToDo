namespace Users.Contracts;

/// <summary>
/// Command to add a new user to the system.
/// </summary>
/// <param name="Email">The user's email address.</param>
/// <param name="UserName">The user's chosen username.</param>
public sealed record AddUserCommand(string Email, string UserName);

/// <summary>
/// Result of successfully adding a new user to the system.
/// </summary>
/// <param name="UserId">The unique identifier of the created user.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="UserName">The user's username.</param>
/// <param name="CreatedAt">The date and time when the user was created.</param>
public sealed record AddUserResult(Guid UserId, string Email, string UserName, DateTime CreatedAt);
