namespace Users.Contracts;

/// <summary>
/// Command to add a new user to the system.
/// </summary>
public sealed record AddUserCommand
{
    /// <summary>
    /// The user's email address.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// The user's chosen username.
    /// </summary>
    public required string UserName { get; init; }
}

/// <summary>
/// Result of successfully adding a new user to the system.
/// </summary>
public sealed record AddUserResult
{
    /// <summary>
    /// The unique identifier of the created user.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// The user's email address.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// The user's username.
    /// </summary>
    public string UserName { get; init; } = string.Empty;

    /// <summary>
    /// The date and time when the user was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }
}
