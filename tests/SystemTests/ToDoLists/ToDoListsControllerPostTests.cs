using System.Net;
using System.Text;
using SystemTests.Users;

namespace SystemTests.ToDoLists;

/// <summary>
/// System tests for ToDoLists POST endpoints.
/// </summary>
public class ToDoListsControllerPostTests : BaseSystemTest
{
    public ToDoListsControllerPostTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }


    [Fact]
    public async Task PostToDoLists_WithValidData_ReturnsCreatedWithToDoListId()
    {
        // Arrange
        Guid userId = await UsersTestHelper.CreateUserAsync(HttpClient);
        object request = new { UserId = userId.ToString(), Title = "My Shopping List" };

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/todo-lists", ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        CreateToDoListApiResponse apiResult = await FromJsonAsync<CreateToDoListApiResponse>(response);
        Assert.NotEqual(Guid.Empty.ToString(), apiResult.Id);
        Assert.Equal(userId.ToString(), apiResult.UserId);
        Assert.Equal("My Shopping List", apiResult.Title);
        Assert.Equal(FakeTimeProvider.GetUtcNow().UtcDateTime, apiResult.CreatedAt);

        // Verify Location header is set correctly
        Assert.NotNull(response.Headers.Location);
        Assert.Contains(apiResult.Id, response.Headers.Location.ToString());
    }

    [Fact]
    public async Task PostToDoLists_WithEmptyTitle_ReturnsBadRequest()
    {
        // Arrange
        Guid userId = await UsersTestHelper.CreateUserAsync(HttpClient);
        object request = new { UserId = userId.ToString(), Title = "" };

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/todo-lists", ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostToDoLists_WithWhitespaceOnlyTitle_ReturnsBadRequest()
    {
        // Arrange
        Guid userId = await UsersTestHelper.CreateUserAsync(HttpClient);
        object request = new { UserId = userId.ToString(), Title = "   " };

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/todo-lists", ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostToDoLists_WithTabOnlyTitle_ReturnsBadRequest()
    {
        // Arrange
        Guid userId = await UsersTestHelper.CreateUserAsync(HttpClient);
        object request = new { UserId = userId.ToString(), Title = "\t\t" };

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/todo-lists", ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostToDoLists_WithEmptyUserId_ReturnsBadRequest()
    {
        // Arrange
        object request = new { UserId = "", Title = "My Shopping List" };

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/todo-lists", ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostToDoLists_WithInvalidGuidUserId_ReturnsBadRequest()
    {
        // Arrange
        object request = new { UserId = "invalid-guid", Title = "My Shopping List" };

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/todo-lists", ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostToDoLists_WithNullTitle_ReturnsBadRequest()
    {
        // Arrange
        Guid userId = await UsersTestHelper.CreateUserAsync(HttpClient);
        string requestBody = $$$"""{"userId": "{{{userId}}}", "title": null}""";
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/todo-lists", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostToDoLists_WithNullUserId_ReturnsBadRequest()
    {
        // Arrange
        string requestBody = """{"userId": null, "title": "My Todo List"}""";
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/todo-lists", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostToDoLists_WithVeryLongTitle_ReturnsBadRequest()
    {
        // Arrange - Create title with 201+ characters (exceeds 200 character limit)
        Guid userId = await UsersTestHelper.CreateUserAsync(HttpClient);
        string longTitle = new string('a', 201);
        object request = new { UserId = userId.ToString(), Title = longTitle };

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/todo-lists", ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostToDoLists_WithTitleExactly200Characters_ReturnsCreated()
    {
        // Arrange - Title at maximum boundary (200 characters)
        Guid userId = await UsersTestHelper.CreateUserAsync(HttpClient);
        string maxTitle = new string('a', 200);
        object request = new { UserId = userId.ToString(), Title = maxTitle };

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/todo-lists", ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PostToDoLists_WithMalformedJson_ReturnsBadRequest()
    {
        // Arrange - Missing closing brace
        string malformedJson = $"{{\"userId\": \"{Guid.NewGuid()}\", \"title\": \"My List\" ";
        var content = new StringContent(malformedJson, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/todo-lists", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostToDoLists_WithMissingTitleProperty_ReturnsBadRequest()
    {
        // Arrange - Missing title property completely
        string requestBody = $"{{\"userId\": \"{Guid.NewGuid()}\"}}";
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/todo-lists", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostToDoLists_WithMissingUserIdProperty_ReturnsBadRequest()
    {
        // Arrange - Missing userId property completely
        string requestBody = """{"title": "My Todo List"}""";
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/todo-lists", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostToDoLists_WithEmptyJsonObject_ReturnsBadRequest()
    {
        // Arrange - Completely empty JSON object
        string requestBody = """{}""";
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/todo-lists", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostToDoLists_WithWrongContentType_ReturnsUnsupportedMediaType()
    {
        // Arrange - Using XML content type instead of JSON
        Guid userId = await UsersTestHelper.CreateUserAsync(HttpClient);
        object request = new { UserId = userId.ToString(), Title = "My Shopping List" };
        string json = System.Text.Json.JsonSerializer.Serialize(request, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
        var content = new StringContent(json, Encoding.UTF8, "application/xml");

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/todo-lists", content);

        // Assert
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
    }

    [Fact]
    public async Task PostToDoLists_WithMissingContentType_ReturnsBadRequest()
    {
        // Arrange - No content type specified
        object request = new { UserId = Guid.NewGuid().ToString(), Title = "My Shopping List" };
        string json = System.Text.Json.JsonSerializer.Serialize(request, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
        var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = null;

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/todo-lists", content);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.UnsupportedMediaType);
    }

    [Fact]
    public async Task PutToDoLists_OnPostEndpoint_ReturnsMethodNotAllowed()
    {
        // Arrange
        object request = new { UserId = Guid.NewGuid().ToString(), Title = "My Shopping List" };

        // Act
        HttpResponseMessage response = await HttpClient.PutAsync("/api/v1/todo-lists", ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task PostToDoLists_WithMalformedJsonMissingQuotes_ReturnsBadRequest()
    {
        // Arrange - JSON with unquoted property names
        string malformedJson = $"{{userId: \"{Guid.NewGuid()}\", title: \"My List\"}}";
        var content = new StringContent(malformedJson, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/todo-lists", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostToDoLists_WithMalformedJsonExtraComma_ReturnsBadRequest()
    {
        // Arrange - JSON with trailing comma
        string malformedJson = $"{{\"userId\": \"{Guid.NewGuid()}\", \"title\": \"My List\",}}";
        var content = new StringContent(malformedJson, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/todo-lists", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostToDoLists_WithInvalidJsonStructure_ReturnsBadRequest()
    {
        // Arrange - Invalid JSON structure
        string invalidJson = """["userId", "title"]""";
        var content = new StringContent(invalidJson, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/todo-lists", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostToDoLists_WithTitleContainingSpecialCharacters_ReturnsCreated()
    {
        // Arrange - Title with special characters should be accepted
        Guid userId = await UsersTestHelper.CreateUserAsync(HttpClient);
        object request = new { UserId = userId.ToString(), Title = "Shopping List! @#$%^&*()_+-={}[]|\\:;\"'<>?,./ " };

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/todo-lists", ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PostToDoLists_WithTitleContainingUnicodeCharacters_ReturnsCreated()
    {
        // Arrange - Title with unicode characters should be accepted
        Guid userId = await UsersTestHelper.CreateUserAsync(HttpClient);
        object request = new { UserId = userId.ToString(), Title = "Liste de courses 🛒 こんにちは مرحبا" };

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/todo-lists", ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PostToDoLists_WithNonExistentUser_ReturnsNotFound()
    {
        // Arrange - Use a random GUID that doesn't exist in the system
        string nonExistentUserId = Guid.NewGuid().ToString();
        object request = new { UserId = nonExistentUserId, Title = "My Shopping List" };

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/todo-lists", ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
