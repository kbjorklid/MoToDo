using Base.Domain;
using Base.Domain.Result;

namespace ToDoLists.Domain;

/// <summary>
/// Represents query criteria for searching and filtering todo lists with validation and factory methods for common scenarios.
/// </summary>
public readonly record struct ToDoListQueryCriteria
{
    public static class Codes
    {
        public const string InvalidUserId = "ToDoListQueryCriteria.InvalidUserId";
    }

    public PagingParameters PagingParameters { get; }
    public UserId UserId { get; }
    public ToDoListsSortBy SortBy { get; }
    public bool Ascending { get; }

    private ToDoListQueryCriteria(PagingParameters pagingParameters, UserId userId, ToDoListsSortBy sortBy, bool ascending)
    {
        PagingParameters = pagingParameters;
        UserId = userId;
        SortBy = sortBy;
        Ascending = ascending;
    }

    /// <summary>
    /// Creates a new ToDoListQueryCriteria with full control over all parameters (used internally by builder).
    /// </summary>
    private static Result<ToDoListQueryCriteria> Create(PagingParameters pagingParameters, UserId userId, ToDoListsSortBy sortBy, bool ascending)
    {
        return new ToDoListQueryCriteria(pagingParameters, userId, sortBy, ascending);
    }

    /// <summary>
    /// Creates a new fluent builder for ToDoListQueryCriteria.
    /// </summary>
    /// <param name="pagingParameters">The paging parameters for the query.</param>
    /// <param name="userId">The user ID to filter todo lists by.</param>
    /// <returns>A new ToDoListQueryCriteriaBuilder instance.</returns>
    public static ToDoListQueryCriteriaBuilder Builder(PagingParameters pagingParameters, UserId userId)
    {
        return new ToDoListQueryCriteriaBuilder(pagingParameters, userId);
    }

    /// <summary>
    /// Fluent builder for ToDoListQueryCriteria with method chaining.
    /// </summary>
    public class ToDoListQueryCriteriaBuilder
    {
        private readonly PagingParameters _pagingParameters;
        private readonly UserId _userId;
        private ToDoListsSortBy _sortBy = ToDoListsSortBy.CreatedAt;
        private bool _ascending = true;

        internal ToDoListQueryCriteriaBuilder(PagingParameters pagingParameters, UserId userId)
        {
            _pagingParameters = pagingParameters;
            _userId = userId;
        }

        /// <summary>
        /// Sets the sort field and direction.
        /// </summary>
        /// <param name="sortBy">The field to sort by.</param>
        /// <param name="ascending">Whether to sort in ascending order (default: true).</param>
        /// <returns>The builder instance for method chaining.</returns>
        public ToDoListQueryCriteriaBuilder WithSortBy(ToDoListsSortBy sortBy, bool ascending = true)
        {
            _sortBy = sortBy;
            _ascending = ascending;
            return this;
        }

        /// <summary>
        /// Sets the sort direction to descending.
        /// </summary>
        /// <returns>The builder instance for method chaining.</returns>
        public ToDoListQueryCriteriaBuilder Descending()
        {
            _ascending = false;
            return this;
        }

        /// <summary>
        /// Builds the ToDoListQueryCriteria with validation.
        /// </summary>
        /// <returns>A Result containing the ToDoListQueryCriteria if valid, or an error if validation fails.</returns>
        public Result<ToDoListQueryCriteria> Build()
        {
            return Create(_pagingParameters, _userId, _sortBy, _ascending);
        }
    }
}
