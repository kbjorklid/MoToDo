namespace Users.Contracts;

/// <summary>
/// Command to delete an existing user from the system.
/// </summary>
/// <param name="UserId">The unique identifier of the user to delete.</param>
public sealed record DeleteUserCommand(string UserId);
