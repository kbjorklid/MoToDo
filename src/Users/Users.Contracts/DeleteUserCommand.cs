namespace Users.Contracts;

/// <summary>
/// Command to delete an existing user from the system.
/// </summary>
public sealed record DeleteUserCommand
{
    /// <summary>
    /// The unique identifier of the user to delete.
    /// </summary>
    public required string UserId { get; init; }
}
