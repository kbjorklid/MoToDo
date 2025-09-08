using System.Net;
using System.Text;
using System.Text.Json;
using SystemTests.TestObjectBuilders;
using SystemTests.Users;
using ToDoLists.Contracts;

namespace SystemTests.ToDoLists;

// API response DTOs for system tests
public sealed record CreateToDoListApiResponse(string Id, string UserId, string Title, DateTime CreatedAt);
public sealed record AddToDoApiResponse(string Id, string Title, bool IsCompleted, DateTime CreatedAt, DateTime? CompletedAt);
public sealed record UpdateToDoApiResponse(string Id, string Title, bool IsCompleted, DateTime CreatedAt, DateTime? CompletedAt);
public sealed record GetToDoListsApiResponse(IReadOnlyList<ToDoListSummaryApiDto> Data, PaginationApiInfo Pagination);
public sealed record ToDoListSummaryApiDto(string Id, string Title, int TodoCount, DateTime CreatedAt, DateTime? UpdatedAt);
public sealed record ToDoListDetailApiResponse(string Id, string Title, ToDoApiDto[] Todos, int TodoCount, DateTime CreatedAt, DateTime? UpdatedAt);
public sealed record ToDoApiDto(string Id, string Title, bool IsCompleted, DateTime CreatedAt, DateTime? CompletedAt);
public sealed record PaginationApiInfo(int TotalItems, int TotalPages, int CurrentPage, int Limit);

/// <summary>
/// Helper methods for ToDoLists system tests.
/// </summary>
public static class ToDoListTestHelper
{
    /// <summary>
    /// Creates a todo list via the API and returns the created result.
    /// </summary>
    public static async Task<CreateToDoListResult> CreateToDoListAsync(HttpClient httpClient)
    {
        Guid userId = await UserTestHelper.CreateUserAsync(httpClient);
        object request = new CreateToDoListCommandBuilder()
            .WithUserId(userId.ToString())
            .BuildApiRequest();
        HttpResponseMessage response = await httpClient.PostAsync("/api/v1/todo-lists", ToJsonContent(request));

        if (response.StatusCode != HttpStatusCode.Created)
            throw new InvalidOperationException($"Failed to create todo list. Status: {response.StatusCode}");

        CreateToDoListApiResponse apiResponse = await FromJsonAsync<CreateToDoListApiResponse>(response);

        // Convert API response back to contract result for backward compatibility
        return new CreateToDoListResult
        {
            ToDoListId = Guid.Parse(apiResponse.Id),
            UserId = Guid.Parse(apiResponse.UserId),
            Title = apiResponse.Title,
            CreatedAt = apiResponse.CreatedAt
        };
    }

    /// <summary>
    /// Creates a todo list with a specific title via the API and returns the created result.
    /// </summary>
    public static async Task<CreateToDoListResult> CreateToDoListAsync(HttpClient httpClient, string title)
    {
        Guid userId = await UserTestHelper.CreateUserAsync(httpClient);
        object request = new CreateToDoListCommandBuilder()
            .WithUserId(userId.ToString())
            .WithTitle(title)
            .BuildApiRequest();
        HttpResponseMessage response = await httpClient.PostAsync("/api/v1/todo-lists", ToJsonContent(request));

        if (response.StatusCode != HttpStatusCode.Created)
            throw new InvalidOperationException($"Failed to create todo list. Status: {response.StatusCode}");

        CreateToDoListApiResponse apiResponse = await FromJsonAsync<CreateToDoListApiResponse>(response);

        // Convert API response back to contract result for backward compatibility
        return new CreateToDoListResult
        {
            ToDoListId = Guid.Parse(apiResponse.Id),
            UserId = Guid.Parse(apiResponse.UserId),
            Title = apiResponse.Title,
            CreatedAt = apiResponse.CreatedAt
        };
    }

    /// <summary>
    /// Creates a todo list for a specific user via the API and returns the created result.
    /// </summary>
    public static async Task<CreateToDoListResult> CreateToDoListAsync(HttpClient httpClient, Guid userId, string title)
    {
        object request = new CreateToDoListCommandBuilder()
            .WithUserId(userId.ToString())
            .WithTitle(title)
            .BuildApiRequest();
        HttpResponseMessage response = await httpClient.PostAsync("/api/v1/todo-lists", ToJsonContent(request));

        if (response.StatusCode != HttpStatusCode.Created)
            throw new InvalidOperationException($"Failed to create todo list. Status: {response.StatusCode}");

        CreateToDoListApiResponse apiResponse = await FromJsonAsync<CreateToDoListApiResponse>(response);

        // Convert API response back to contract result for backward compatibility
        return new CreateToDoListResult
        {
            ToDoListId = Guid.Parse(apiResponse.Id),
            UserId = Guid.Parse(apiResponse.UserId),
            Title = apiResponse.Title,
            CreatedAt = apiResponse.CreatedAt
        };
    }

    /// <summary>
    /// Creates a todo list for a specific user with default title via the API and returns the created result.
    /// </summary>
    public static async Task<CreateToDoListResult> CreateToDoListAsync(HttpClient httpClient, Guid userId)
    {
        object request = new CreateToDoListCommandBuilder()
            .WithUserId(userId.ToString())
            .BuildApiRequest();
        HttpResponseMessage response = await httpClient.PostAsync("/api/v1/todo-lists", ToJsonContent(request));

        if (response.StatusCode != HttpStatusCode.Created)
            throw new InvalidOperationException($"Failed to create todo list. Status: {response.StatusCode}");

        CreateToDoListApiResponse apiResponse = await FromJsonAsync<CreateToDoListApiResponse>(response);

        // Convert API response back to contract result for backward compatibility
        return new CreateToDoListResult
        {
            ToDoListId = Guid.Parse(apiResponse.Id),
            UserId = Guid.Parse(apiResponse.UserId),
            Title = apiResponse.Title,
            CreatedAt = apiResponse.CreatedAt
        };
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

        AddToDoApiResponse apiResponse = await FromJsonAsync<AddToDoApiResponse>(response);

        // Convert API response back to contract result for backward compatibility
        return new AddToDoResult
        {
            Id = Guid.Parse(apiResponse.Id),
            Title = apiResponse.Title,
            IsCompleted = apiResponse.IsCompleted,
            CreatedAt = apiResponse.CreatedAt,
            CompletedAt = apiResponse.CompletedAt
        };
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

        AddToDoApiResponse apiResponse = await FromJsonAsync<AddToDoApiResponse>(response);

        // Convert API response back to contract result for backward compatibility
        return new AddToDoResult
        {
            Id = Guid.Parse(apiResponse.Id),
            Title = apiResponse.Title,
            IsCompleted = apiResponse.IsCompleted,
            CreatedAt = apiResponse.CreatedAt,
            CompletedAt = apiResponse.CompletedAt
        };
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
