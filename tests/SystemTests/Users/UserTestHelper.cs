using System.Text;
using System.Text.Json;
using SystemTests.TestObjectBuilders;
using Users.Contracts;

namespace SystemTests.Users;

/// <summary>
/// Helper class for Users-related system test operations.
/// </summary>
public static class UserTestHelper
{
    /// <summary>
    /// Creates a user via API and returns the user ID.
    /// </summary>
    /// <param name="httpClient">HTTP client for API calls</param>
    /// <param name="command">Command to create the user</param>
    /// <returns>The created user's ID</returns>
    public static async Task<Guid> CreateUserAsync(HttpClient httpClient, AddUserCommand command)
    {
        HttpResponseMessage createResponse = await httpClient.PostAsync("/api/v1/users", ToJsonContent(command));
        AddUserResult createdUser = await FromJsonAsync<AddUserResult>(createResponse);
        return createdUser.UserId;
    }

    /// <summary>
    /// Creates a user with default test data via API and returns the user ID.
    /// </summary>
    /// <param name="httpClient">HTTP client for API calls</param>
    /// <returns>The created user's ID</returns>
    public static async Task<Guid> CreateUserAsync(HttpClient httpClient)
    {
        AddUserCommand command = new AddUserCommandBuilder().Build();
        return await CreateUserAsync(httpClient, command);
    }

    /// <summary>
    /// Converts an object to JSON content for HTTP requests.
    /// </summary>
    /// <param name="obj">Object to serialize</param>
    /// <returns>StringContent with JSON payload</returns>
    private static StringContent ToJsonContent(object obj)
    {
        string json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    /// <summary>
    /// Deserializes JSON response to specified type.
    /// </summary>
    /// <typeparam name="T">Type to deserialize to</typeparam>
    /// <param name="response">HTTP response message</param>
    /// <returns>Deserialized object</returns>
    private static async Task<T> FromJsonAsync<T>(HttpResponseMessage response)
    {
        string content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;
    }
}
