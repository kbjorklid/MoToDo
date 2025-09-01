using System.Net;
using System.Text;
using System.Text.Json;
using SystemTests.TestObjectBuilders;
using ToDoLists.Contracts;

namespace SystemTests.ToDoLists;

/// <summary>
/// Helper methods for ToDoLists system tests.
/// </summary>
public static class ToDoListsTestHelper
{
    /// <summary>
    /// Creates a todo list via the API and returns the created result.
    /// </summary>
    public static async Task<CreateToDoListResult> CreateToDoListAsync(HttpClient httpClient)
    {
        CreateToDoListCommand command = new CreateToDoListCommandBuilder().Build();
        HttpResponseMessage response = await httpClient.PostAsync("/api/v1/todo-lists", ToJsonContent(command));

        if (response.StatusCode != HttpStatusCode.Created)
            throw new InvalidOperationException($"Failed to create todo list. Status: {response.StatusCode}");

        return await FromJsonAsync<CreateToDoListResult>(response);
    }

    /// <summary>
    /// Creates a todo list with a specific title via the API and returns the created result.
    /// </summary>
    public static async Task<CreateToDoListResult> CreateToDoListAsync(HttpClient httpClient, string title)
    {
        CreateToDoListCommand command = new CreateToDoListCommandBuilder()
            .WithTitle(title)
            .Build();
        HttpResponseMessage response = await httpClient.PostAsync("/api/v1/todo-lists", ToJsonContent(command));

        if (response.StatusCode != HttpStatusCode.Created)
            throw new InvalidOperationException($"Failed to create todo list. Status: {response.StatusCode}");

        return await FromJsonAsync<CreateToDoListResult>(response);
    }

    /// <summary>
    /// Creates a todo list for a specific user via the API and returns the created result.
    /// </summary>
    public static async Task<CreateToDoListResult> CreateToDoListAsync(HttpClient httpClient, Guid userId, string title)
    {
        CreateToDoListCommand command = new CreateToDoListCommandBuilder()
            .WithUserId(userId.ToString())
            .WithTitle(title)
            .Build();
        HttpResponseMessage response = await httpClient.PostAsync("/api/v1/todo-lists", ToJsonContent(command));

        if (response.StatusCode != HttpStatusCode.Created)
            throw new InvalidOperationException($"Failed to create todo list. Status: {response.StatusCode}");

        return await FromJsonAsync<CreateToDoListResult>(response);
    }

    /// <summary>
    /// Creates a todo list for a specific user with default title via the API and returns the created result.
    /// </summary>
    public static async Task<CreateToDoListResult> CreateToDoListAsync(HttpClient httpClient, Guid userId)
    {
        CreateToDoListCommand command = new CreateToDoListCommandBuilder()
            .WithUserId(userId.ToString())
            .Build();
        HttpResponseMessage response = await httpClient.PostAsync("/api/v1/todo-lists", ToJsonContent(command));

        if (response.StatusCode != HttpStatusCode.Created)
            throw new InvalidOperationException($"Failed to create todo list. Status: {response.StatusCode}");

        return await FromJsonAsync<CreateToDoListResult>(response);
    }

    /// <summary>
    /// Serializes an object to JSON with camelCase naming policy to match API conventions.
    /// </summary>
    private static StringContent ToJsonContent(object obj)
    {
        string json = JsonSerializer.Serialize(obj,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    /// <summary>
    /// Adds a todo item to an existing todo list via the API and returns the created result.
    /// </summary>
    public static async Task<AddToDoResult> AddToDoAsync(HttpClient httpClient, Guid toDoListId, string title)
    {
        object request = new AddToDoRequestBuilder()
            .WithTitle(title)
            .Build();
        HttpResponseMessage response = await httpClient.PostAsync($"/api/v1/todo-lists/{toDoListId}/todos", ToJsonContent(request));

        if (response.StatusCode != HttpStatusCode.Created)
            throw new InvalidOperationException($"Failed to add todo. Status: {response.StatusCode}");

        return await FromJsonAsync<AddToDoResult>(response);
    }

    /// <summary>
    /// Adds a todo item to an existing todo list with default title via the API and returns the created result.
    /// </summary>
    public static async Task<AddToDoResult> AddToDoAsync(HttpClient httpClient, Guid toDoListId)
    {
        object request = new AddToDoRequestBuilder().Build();
        HttpResponseMessage response = await httpClient.PostAsync($"/api/v1/todo-lists/{toDoListId}/todos", ToJsonContent(request));

        if (response.StatusCode != HttpStatusCode.Created)
            throw new InvalidOperationException($"Failed to add todo. Status: {response.StatusCode}");

        return await FromJsonAsync<AddToDoResult>(response);
    }

    /// <summary>
    /// Deserializes JSON response to the specified type with camelCase naming policy.
    /// </summary>
    private static async Task<T> FromJsonAsync<T>(HttpResponseMessage response)
    {
        string json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;
    }
}
