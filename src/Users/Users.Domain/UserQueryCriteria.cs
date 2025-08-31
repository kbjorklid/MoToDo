using Base.Domain;
using Base.Domain.Result;

namespace Users.Domain;

/// <summary>
/// Represents query criteria for searching and filtering users with validation and factory methods for common scenarios.
/// </summary>
public readonly record struct UserQueryCriteria
{
    public static class Codes
    {
        public const string InvalidPagingParameters = "UserQueryCriteria.InvalidPagingParameters";
        public const string EmptyEmailFilter = "UserQueryCriteria.EmptyEmailFilter";
        public const string EmptyUserNameFilter = "UserQueryCriteria.EmptyUserNameFilter";
    }

    public PagingParameters PagingParameters { get; }
    public string? EmailFilter { get; }
    public string? UserNameFilter { get; }
    public UsersSortBy SortBy { get; }
    public bool Ascending { get; }

    private UserQueryCriteria(PagingParameters pagingParameters, string? emailFilter, string? userNameFilter, UsersSortBy sortBy, bool ascending)
    {
        PagingParameters = pagingParameters;
        EmailFilter = emailFilter;
        UserNameFilter = userNameFilter;
        SortBy = sortBy;
        Ascending = ascending;
    }

    /// <summary>
    /// Creates a new UserQueryCriteria with full control over all parameters (used internally by builder).
    /// </summary>
    private static Result<UserQueryCriteria> Create(PagingParameters pagingParameters, string? emailFilter, string? userNameFilter, UsersSortBy sortBy, bool ascending)
    {
        // Validate email filter - if provided, it cannot be empty or whitespace
        if (emailFilter is not null && string.IsNullOrWhiteSpace(emailFilter))
            return new Error(Codes.EmptyEmailFilter, "Email filter cannot be empty or whitespace when provided.", ErrorType.Validation);

        // Validate username filter - if provided, it cannot be empty or whitespace
        if (userNameFilter is not null && string.IsNullOrWhiteSpace(userNameFilter))
            return new Error(Codes.EmptyUserNameFilter, "Username filter cannot be empty or whitespace when provided.", ErrorType.Validation);

        return new UserQueryCriteria(pagingParameters, emailFilter, userNameFilter, sortBy, ascending);
    }

    /// <summary>
    /// Creates a new fluent builder for UserQueryCriteria.
    /// </summary>
    /// <param name="pagingParameters">The paging parameters for the query.</param>
    /// <returns>A new UserQueryCriteriaBuilder instance.</returns>
    public static UserQueryCriteriaBuilder Builder(PagingParameters pagingParameters)
    {
        return new UserQueryCriteriaBuilder(pagingParameters);
    }

    /// <summary>
    /// Fluent builder for UserQueryCriteria with method chaining.
    /// </summary>
    public class UserQueryCriteriaBuilder
    {
        private readonly PagingParameters _pagingParameters;
        private string? _emailFilter;
        private string? _userNameFilter;
        private UsersSortBy _sortBy = UsersSortBy.CreatedAt;
        private bool _ascending = true;

        internal UserQueryCriteriaBuilder(PagingParameters pagingParameters)
        {
            _pagingParameters = pagingParameters;
        }

        /// <summary>
        /// Adds an email filter to the query criteria.
        /// </summary>
        /// <param name="emailFilter">The email filter (can be partial match).</param>
        /// <returns>The builder instance for method chaining.</returns>
        public UserQueryCriteriaBuilder WithEmailFilter(string emailFilter)
        {
            _emailFilter = emailFilter;
            return this;
        }

        /// <summary>
        /// Adds a username filter to the query criteria.
        /// </summary>
        /// <param name="userNameFilter">The username filter (can be partial match).</param>
        /// <returns>The builder instance for method chaining.</returns>
        public UserQueryCriteriaBuilder WithUserNameFilter(string userNameFilter)
        {
            _userNameFilter = userNameFilter;
            return this;
        }

        /// <summary>
        /// Sets the sort field and direction.
        /// </summary>
        /// <param name="sortBy">The field to sort by.</param>
        /// <param name="ascending">Whether to sort in ascending order (default: true).</param>
        /// <returns>The builder instance for method chaining.</returns>
        public UserQueryCriteriaBuilder WithSortBy(UsersSortBy sortBy, bool ascending = true)
        {
            _sortBy = sortBy;
            _ascending = ascending;
            return this;
        }

        /// <summary>
        /// Sets the sort direction to descending.
        /// </summary>
        /// <returns>The builder instance for method chaining.</returns>
        public UserQueryCriteriaBuilder Descending()
        {
            _ascending = false;
            return this;
        }

        /// <summary>
        /// Builds the UserQueryCriteria with validation.
        /// </summary>
        /// <returns>A Result containing the UserQueryCriteria if valid, or an error if validation fails.</returns>
        public Result<UserQueryCriteria> Build()
        {
            return Create(_pagingParameters, _emailFilter, _userNameFilter, _sortBy, _ascending);
        }
    }
}
