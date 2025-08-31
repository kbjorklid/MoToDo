using Base.Domain.Result;

namespace Users.Domain.Tests.TestObjectBuilders;

/// <summary>
/// Test Object Builder for creating UserName instances in tests.
/// </summary>
public class UserNameBuilder
{
    private string _value = "testuser";

    public UserNameBuilder WithValue(string value)
    {
        _value = value;
        return this;
    }

    public UserName Build()
    {
        Result<UserName> result = UserName.Create(_value);
        if (result.IsFailure)
            throw new InvalidOperationException($"Failed to build UserName: {result.Error.Description}");
        return result.Value;
    }
}
