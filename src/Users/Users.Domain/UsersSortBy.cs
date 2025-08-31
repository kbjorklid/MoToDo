namespace Users.Domain;

/// <summary>
/// Defines the available sorting options for User queries.
/// </summary>
public enum UsersSortBy
{
    /// <summary>
    /// Sort by the date and time the user was created.
    /// </summary>
    CreatedAt,

    /// <summary>
    /// Sort by the user's email address.
    /// </summary>
    Email,

    /// <summary>
    /// Sort by the user's username.
    /// </summary>
    UserName,

    /// <summary>
    /// Sort by the date and time the user last logged in.
    /// </summary>
    LastLoginAt
}
