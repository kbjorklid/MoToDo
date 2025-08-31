namespace SystemTests.TestObjectBuilders;

/// <summary>
/// Test Object Builder for creating AddToDoRequest instances in tests.
/// </summary>
public class AddToDoRequestBuilder
{
    private static int _counter = 0;
    private string _title = $"Test Todo Item {++_counter}";

    public AddToDoRequestBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public object Build()
    {
        return new { Title = _title };
    }
}
