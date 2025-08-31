namespace Users.Contracts;

/// <summary>
/// Query to retrieve a user by their unique identifier.
/// </summary>
/// <param name="UserId">The unique identifier of the user to retrieve.</param>
public sealed record GetUserByIdQuery(string UserId);
