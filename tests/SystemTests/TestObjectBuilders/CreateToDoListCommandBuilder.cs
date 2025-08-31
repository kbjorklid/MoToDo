using ToDoLists.Contracts;

namespace SystemTests.TestObjectBuilders;

/// <summary>
/// Test Object Builder for creating CreateToDoListCommand instances in tests.
/// </summary>
public class CreateToDoListCommandBuilder
{
    private static int _counter = 0;
    private string _userId = Guid.NewGuid().ToString();
    private string _title = $"Test Todo List {++_counter}";

    public CreateToDoListCommandBuilder WithUserId(string userId)
    {
        _userId = userId;
        return this;
    }

    public CreateToDoListCommandBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public CreateToDoListCommand Build()
    {
        return new CreateToDoListCommand(_userId, _title);
    }
}
