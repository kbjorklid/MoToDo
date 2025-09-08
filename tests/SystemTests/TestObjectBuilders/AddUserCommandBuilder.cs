using Users.Contracts;

namespace SystemTests.TestObjectBuilders;

/// <summary>
/// Test Object Builder for creating AddUserCommand instances in tests.
/// </summary>
public class AddUserCommandBuilder
{
    private static int _counter = 0;
    private string _email = $"test{++_counter}@example.com";
    private string _userName = $"testuser{_counter}";

    public AddUserCommandBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public AddUserCommandBuilder WithUserName(string userName)
    {
        _userName = userName;
        return this;
    }

    public AddUserCommand Build()
    {
        return new AddUserCommand
        {
            Email = _email,
            UserName = _userName
        };
    }
}
