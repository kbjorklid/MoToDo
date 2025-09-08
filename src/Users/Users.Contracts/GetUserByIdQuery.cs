namespace Users.Contracts;

/// <summary>
/// Query to retrieve a user by their unique identifier.
/// </summary>
public sealed record GetUserByIdQuery
{
    /// <summary>
    /// The unique identifier of the user to retrieve.
    /// </summary>
    public required string UserId { get; init; }
}
