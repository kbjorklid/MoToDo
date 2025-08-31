namespace Base.Domain;

/// <summary>
/// Represents a paginated result containing data and pagination metadata.
/// </summary>
/// <typeparam name="T">The type of items in the paged result.</typeparam>
public sealed class PagedResult<T>
{
    /// <summary>
    /// The collection of items for the current page.
    /// </summary>
    public IReadOnlyList<T> Data { get; }

    /// <summary>
    /// The total number of items across all pages.
    /// </summary>
    public int TotalItems { get; }

    /// <summary>
    /// The total number of pages based on the total items and page size.
    /// </summary>
    public int TotalPages { get; }

    /// <summary>
    /// The current page number.
    /// </summary>
    public int CurrentPage { get; }

    /// <summary>
    /// The number of items per page (page size).
    /// </summary>
    public int Limit { get; }

    /// <summary>
    /// Indicates whether there is a previous page available.
    /// </summary>
    public bool HasPreviousPage => CurrentPage > 1;

    /// <summary>
    /// Indicates whether there is a next page available.
    /// </summary>
    public bool HasNextPage => CurrentPage < TotalPages;

    private PagedResult(IReadOnlyList<T> data, int totalItems, int currentPage, int limit)
    {
        ArgumentNullException.ThrowIfNull(data);
        Data = data;
        TotalItems = totalItems;
        CurrentPage = currentPage;
        Limit = limit;
        TotalPages = CalculateTotalPages(totalItems, limit);
    }

    /// <summary>
    /// Creates a new PagedResult instance.
    /// </summary>
    /// <param name="data">The collection of items for the current page.</param>
    /// <param name="totalItems">The total number of items across all pages.</param>
    /// <param name="pagingParameters">The paging parameters used for this result.</param>
    /// <returns>A new PagedResult instance.</returns>
    public static PagedResult<T> Create(IReadOnlyList<T> data, int totalItems, PagingParameters pagingParameters)
    {
        return new PagedResult<T>(data, totalItems, pagingParameters.Page, pagingParameters.Limit);
    }

    /// <summary>
    /// Creates a new PagedResult instance.
    /// </summary>
    /// <param name="data">The collection of items for the current page.</param>
    /// <param name="totalItems">The total number of items across all pages.</param>
    /// <param name="currentPage">The current page number.</param>
    /// <param name="limit">The number of items per page.</param>
    /// <returns>A new PagedResult instance.</returns>
    public static PagedResult<T> Create(IReadOnlyList<T> data, int totalItems, int currentPage, int limit)
    {
        return new PagedResult<T>(data, totalItems, currentPage, limit);
    }

    /// <summary>
    /// Creates an empty PagedResult with no items.
    /// </summary>
    /// <param name="pagingParameters">The paging parameters used for this result.</param>
    /// <returns>An empty PagedResult instance.</returns>
    public static PagedResult<T> Empty(PagingParameters pagingParameters)
    {
        return new PagedResult<T>(Array.Empty<T>(), 0, pagingParameters.Page, pagingParameters.Limit);
    }

    /// <summary>
    /// Creates an empty PagedResult with no items.
    /// </summary>
    /// <param name="currentPage">The current page number.</param>
    /// <param name="limit">The number of items per page.</param>
    /// <returns>An empty PagedResult instance.</returns>
    public static PagedResult<T> Empty(int currentPage, int limit)
    {
        return new PagedResult<T>(Array.Empty<T>(), 0, currentPage, limit);
    }

    private static int CalculateTotalPages(int totalItems, int limit)
    {
        if (totalItems <= 0 || limit <= 0)
            return 0;
        return (int)Math.Ceiling((double)totalItems / limit);
    }
}
