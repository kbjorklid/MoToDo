using Base.Contracts;
using Base.Domain;
using Base.Domain.Result;

namespace Base.Application;

/// <summary>
/// Helper utilities for common pagination operations in the application layer.
/// </summary>
public static class PaginationHelpers
{
    /// <summary>
    /// Creates PagingParameters from nullable query parameters with defaults.
    /// </summary>
    /// <param name="page">Optional page number (defaults to 1).</param>
    /// <param name="limit">Optional limit per page (defaults to 50).</param>
    /// <returns>A Result containing PagingParameters if valid, or an error if validation fails.</returns>
    public static Result<PagingParameters> CreatePagingParameters(int? page, int? limit)
    {
        return PagingParameters.Create(
            page ?? PagingParameters.DefaultPage,
            limit ?? PagingParameters.DefaultLimit
        );
    }

    /// <summary>
    /// Creates a PaginationInfo object from paged data for response DTOs.
    /// </summary>
    /// <param name="totalItems">Total number of items across all pages.</param>
    /// <param name="pagingParameters">The paging parameters used for the query.</param>
    /// <returns>A PaginationInfo object for use in response DTOs.</returns>
    public static PaginationInfo CreatePaginationInfo(int totalItems, PagingParameters pagingParameters)
    {
        int totalPages = (int)Math.Ceiling((double)totalItems / pagingParameters.Limit);
        return new PaginationInfo(totalItems, totalPages, pagingParameters.Page, pagingParameters.Limit);
    }

    /// <summary>
    /// Creates a PaginationInfo object from a PagedResult.
    /// </summary>
    /// <typeparam name="T">The type of items in the paged result.</typeparam>
    /// <param name="pagedResult">The paged result to extract pagination info from.</param>
    /// <returns>A PaginationInfo object for use in response DTOs.</returns>
    public static PaginationInfo CreatePaginationInfo<T>(PagedResult<T> pagedResult)
    {
        return new PaginationInfo(
            pagedResult.TotalItems,
            pagedResult.TotalPages,
            pagedResult.CurrentPage,
            pagedResult.Limit
        );
    }
}
