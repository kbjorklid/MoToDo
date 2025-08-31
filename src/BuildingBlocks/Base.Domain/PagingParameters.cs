using Base.Domain.Result;

namespace Base.Domain;

/// <summary>
/// Represents pagination parameters with validation for page number and page size limits.
/// </summary>
public readonly record struct PagingParameters
{
    public static class Codes
    {
        public const string InvalidPage = "PagingParameters.InvalidPage";
        public const string InvalidLimit = "PagingParameters.InvalidLimit";
    }

    private const int MinPage = 1;
    private const int MinLimit = 1;
    private const int MaxLimit = 100;
    public const int DefaultPage = 1;
    public const int DefaultLimit = 50;

    public int Page { get; }
    public int Limit { get; }

    private PagingParameters(int page, int limit)
    {
        Page = page;
        Limit = limit;
    }

    /// <summary>
    /// Creates a new PagingParameters instance with the specified page and limit values.
    /// </summary>
    /// <param name="page">The page number (minimum 1).</param>
    /// <param name="limit">The number of items per page (minimum 1, maximum 100).</param>
    /// <returns>A Result containing the PagingParameters if valid, or an error if validation fails.</returns>
    public static Result<PagingParameters> Create(int page, int limit)
    {
        if (page < MinPage)
            return new Error(Codes.InvalidPage, $"Page must be at least {MinPage}.", ErrorType.Validation);

        if (limit < MinLimit)
            return new Error(Codes.InvalidLimit, $"Limit must be at least {MinLimit}.", ErrorType.Validation);

        if (limit > MaxLimit)
            return new Error(Codes.InvalidLimit, $"Limit cannot exceed {MaxLimit}.", ErrorType.Validation);

        return new PagingParameters(page, limit);
    }

    /// <summary>
    /// Creates a new PagingParameters instance with default values (page = 1, limit = 50).
    /// </summary>
    /// <returns>A PagingParameters instance with default values.</returns>
    public static PagingParameters Default() => new(DefaultPage, DefaultLimit);

    /// <summary>
    /// Creates a new PagingParameters instance with the specified page and default limit (50).
    /// </summary>
    /// <param name="page">The page number (minimum 1).</param>
    /// <returns>A Result containing the PagingParameters if valid, or an error if validation fails.</returns>
    public static Result<PagingParameters> WithPage(int page) => Create(page, DefaultLimit);

    /// <summary>
    /// Creates a new PagingParameters instance with the specified limit and default page (1).
    /// </summary>
    /// <param name="limit">The number of items per page (minimum 1, maximum 100).</param>
    /// <returns>A Result containing the PagingParameters if valid, or an error if validation fails.</returns>
    public static Result<PagingParameters> WithLimit(int limit) => Create(DefaultPage, limit);

    /// <summary>
    /// Calculates the number of items to skip for database queries.
    /// </summary>
    /// <returns>The number of items to skip.</returns>
    public int Skip => (Page - 1) * Limit;

    /// <summary>
    /// Calculates the total number of pages for the given total item count.
    /// </summary>
    /// <param name="totalItems">The total number of items.</param>
    /// <returns>The total number of pages.</returns>
    public int CalculateTotalPages(int totalItems)
    {
        if (totalItems <= 0)
            return 0;
        return (int)Math.Ceiling((double)totalItems / Limit);
    }
}
