using Base.Domain.Result;

namespace Users.Domain.Tests.TestObjectBuilders;

/// <summary>
/// Test Object Builder for creating User instances in tests.
/// </summary>
public class UserBuilder
{
    private string _email = "test@example.com";
    private string _userName = "testuser";
    private DateTime _createdAt = DateTime.UtcNow;

    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserBuilder WithUserName(string userName)
    {
        _userName = userName;
        return this;
    }

    public UserBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public User Build()
    {
        Result<User> result = User.Register(_email, _userName, _createdAt);
        if (result.IsFailure)
            throw new InvalidOperationException($"Failed to build User: {result.Error.Description}");
        return result.Value;
    }
}
