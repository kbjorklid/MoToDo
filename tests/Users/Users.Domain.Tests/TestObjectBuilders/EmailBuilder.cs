using Base.Domain.Result;

namespace Users.Domain.Tests.TestObjectBuilders;

/// <summary>
/// Test Object Builder for creating Email instances in tests.
/// </summary>
public class EmailBuilder
{
    private string _address = "test@example.com";

    public EmailBuilder WithAddress(string address)
    {
        _address = address;
        return this;
    }

    public Email Build()
    {
        Result<Email> result = Email.Create(_address);
        if (result.IsFailure)
            throw new InvalidOperationException($"Failed to build Email: {result.Error.Description}");
        return result.Value;
    }
}
