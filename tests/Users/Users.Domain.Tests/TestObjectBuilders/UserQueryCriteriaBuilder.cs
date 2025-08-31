using Base.Domain;
using Base.Domain.Result;

namespace Users.Domain.Tests.TestObjectBuilders;

/// <summary>
/// Test Object Builder for creating UserQueryCriteria instances in tests.
/// </summary>
public class UserQueryCriteriaBuilder
{
    private PagingParameters _pagingParameters = new PagingParametersBuilder().Build();
    private string? _emailFilter;
    private string? _userNameFilter;
    private UsersSortBy _sortBy = UsersSortBy.CreatedAt;
    private bool _ascending = true;

    public UserQueryCriteriaBuilder WithPagingParameters(PagingParameters pagingParameters)
    {
        _pagingParameters = pagingParameters;
        return this;
    }

    public UserQueryCriteriaBuilder WithEmailFilter(string emailFilter)
    {
        _emailFilter = emailFilter;
        return this;
    }

    public UserQueryCriteriaBuilder WithUserNameFilter(string userNameFilter)
    {
        _userNameFilter = userNameFilter;
        return this;
    }

    public UserQueryCriteriaBuilder WithSortBy(UsersSortBy sortBy)
    {
        _sortBy = sortBy;
        return this;
    }

    public UserQueryCriteriaBuilder WithAscending(bool ascending)
    {
        _ascending = ascending;
        return this;
    }

    public UserQueryCriteriaBuilder Descending()
    {
        _ascending = false;
        return this;
    }

    public UserQueryCriteria Build()
    {
        UserQueryCriteria.UserQueryCriteriaBuilder criteriaBuilder = UserQueryCriteria.Builder(_pagingParameters);

        if (_emailFilter != null)
            criteriaBuilder.WithEmailFilter(_emailFilter);

        if (_userNameFilter != null)
            criteriaBuilder.WithUserNameFilter(_userNameFilter);

        Result<UserQueryCriteria> result = criteriaBuilder
            .WithSortBy(_sortBy, _ascending)
            .Build();

        if (result.IsFailure)
            throw new InvalidOperationException($"Failed to build UserQueryCriteria: {result.Error.Description}");

        return result.Value;
    }
}
