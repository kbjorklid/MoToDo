using System.Net;
using System.Text;
using SystemTests.TestObjectBuilders;
using ToDoLists.Contracts;

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
        CreateToDoListCommand command = new CreateToDoListCommandBuilder()
            .WithUserId(Guid.NewGuid().ToString())
            .WithTitle("My Shopping List")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/todo-lists", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        CreateToDoListResult result = await FromJsonAsync<CreateToDoListResult>(response);
        Assert.NotEqual(Guid.Empty, result.ToDoListId);
        Assert.Equal(Guid.Parse(command.UserId), result.UserId);
        Assert.Equal("My Shopping List", result.Title);
        Assert.Equal(FakeTimeProvider.GetUtcNow().UtcDateTime, result.CreatedAt);

        // Verify Location header is set correctly
        Assert.NotNull(response.Headers.Location);
        Assert.Contains(result.ToDoListId.ToString(), response.Headers.Location.ToString());
    }

    [Fact]
    public async Task PostToDoLists_WithEmptyTitle_ReturnsBadRequest()
    {
        // Arrange
        CreateToDoListCommand command = new CreateToDoListCommandBuilder()
            .WithTitle("")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/todo-lists", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostToDoLists_WithWhitespaceOnlyTitle_ReturnsBadRequest()
    {
        // Arrange
        CreateToDoListCommand command = new CreateToDoListCommandBuilder()
            .WithTitle("   ")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/todo-lists", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostToDoLists_WithTabOnlyTitle_ReturnsBadRequest()
    {
        // Arrange
        CreateToDoListCommand command = new CreateToDoListCommandBuilder()
            .WithTitle("\t\t")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/todo-lists", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostToDoLists_WithEmptyUserId_ReturnsBadRequest()
    {
        // Arrange
        CreateToDoListCommand command = new CreateToDoListCommandBuilder()
            .WithUserId("")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/todo-lists", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostToDoLists_WithInvalidGuidUserId_ReturnsBadRequest()
    {
        // Arrange
        CreateToDoListCommand command = new CreateToDoListCommandBuilder()
            .WithUserId("invalid-guid")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/todo-lists", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostToDoLists_WithNullTitle_ReturnsBadRequest()
    {
        // Arrange
        string requestBody = $$$"""{"userId": "{{{Guid.NewGuid()}}}", "title": null}""";
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
        string longTitle = new string('a', 201);
        CreateToDoListCommand command = new CreateToDoListCommandBuilder()
            .WithTitle(longTitle)
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/todo-lists", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostToDoLists_WithTitleExactly200Characters_ReturnsCreated()
    {
        // Arrange - Title at maximum boundary (200 characters)
        string maxTitle = new string('a', 200);
        CreateToDoListCommand command = new CreateToDoListCommandBuilder()
            .WithTitle(maxTitle)
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/todo-lists", ToJsonContent(command));

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
        CreateToDoListCommand command = new CreateToDoListCommandBuilder().Build();
        string json = System.Text.Json.JsonSerializer.Serialize(command, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
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
        CreateToDoListCommand command = new CreateToDoListCommandBuilder().Build();
        string json = System.Text.Json.JsonSerializer.Serialize(command, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
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
        CreateToDoListCommand command = new CreateToDoListCommandBuilder().Build();

        // Act
        HttpResponseMessage response = await HttpClient.PutAsync("/api/v1/todo-lists", ToJsonContent(command));

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
        CreateToDoListCommand command = new CreateToDoListCommandBuilder()
            .WithTitle("Shopping List! @#$%^&*()_+-={}[]|\\:;\"'<>?,./")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/todo-lists", content: ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PostToDoLists_WithTitleContainingUnicodeCharacters_ReturnsCreated()
    {
        // Arrange - Title with unicode characters should be accepted
        CreateToDoListCommand command = new CreateToDoListCommandBuilder()
            .WithTitle("Liste de courses 🛒 こんにちは مرحبا")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/todo-lists", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
