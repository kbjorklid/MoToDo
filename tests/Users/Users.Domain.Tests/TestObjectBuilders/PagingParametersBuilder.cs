using Base.Domain;
using Base.Domain.Result;

namespace Users.Domain.Tests.TestObjectBuilders;

/// <summary>
/// Test Object Builder for creating PagingParameters instances in tests.
/// </summary>
public class PagingParametersBuilder
{
    private int _page = PagingParameters.DefaultPage;
    private int _limit = PagingParameters.DefaultLimit;

    public PagingParametersBuilder WithPage(int page)
    {
        _page = page;
        return this;
    }

    public PagingParametersBuilder WithLimit(int limit)
    {
        _limit = limit;
        return this;
    }

    public PagingParameters Build()
    {
        Result<PagingParameters> result = PagingParameters.Create(_page, _limit);
        if (result.IsFailure)
            throw new InvalidOperationException($"Failed to build PagingParameters: {result.Error.Description}");
        return result.Value;
    }
}
