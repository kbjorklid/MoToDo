namespace SystemTests.TestObjectBuilders;

/// <summary>
/// Test Object Builder for creating UpdateToDoRequest instances in tests.
/// </summary>
public class UpdateToDoRequestBuilder
{
    private static int _counter = 0;
    private string? _title = $"Updated Todo Item {++_counter}";
    private bool? _isCompleted = null;

    public UpdateToDoRequestBuilder WithTitle(string? title)
    {
        _title = title;
        return this;
    }

    public UpdateToDoRequestBuilder WithIsCompleted(bool? isCompleted)
    {
        _isCompleted = isCompleted;
        return this;
    }

    public UpdateToDoRequestBuilder WithNoTitle()
    {
        _title = null;
        return this;
    }

    public UpdateToDoRequestBuilder WithNoIsCompleted()
    {
        _isCompleted = null;
        return this;
    }

    public object Build()
    {
        return new { Title = _title, IsCompleted = _isCompleted };
    }
}
