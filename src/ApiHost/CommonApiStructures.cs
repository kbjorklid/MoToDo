namespace ApiHost;

/// <summary>
/// API pagination information.
/// </summary>
/// <param name="TotalItems">Total number of items available.</param>
/// <param name="TotalPages">Total number of pages available.</param>
/// <param name="CurrentPage">Current page number.</param>
/// <param name="Limit">Number of items per page.</param>
public sealed record PaginationApiInfo(
    int TotalItems,
    int TotalPages,
    int CurrentPage,
    int Limit);
