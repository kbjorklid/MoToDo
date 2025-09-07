using System.Net;
using System.Text;
using SystemTests.TestObjectBuilders;
using ToDoLists.Contracts;

namespace SystemTests.ToDoLists;

/// <summary>
/// System tests for POST /todo-lists/{listId}/todos endpoint.
/// </summary>
public class ToDoListControllerPostTodosTests : BaseSystemTest
{
    public ToDoListControllerPostTodosTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }


    [Fact]
    public async Task PostTodosToList_WithValidData_ReturnsCreatedWithTodoDetails()
    {
        // Arrange
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient);
        object request = new AddToDoRequestBuilder()
            .WithTitle("Buy groceries")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync($"/api/v1/todo-lists/{todoList.ToDoListId}/todos", ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        AddToDoApiResponse result = await FromJsonAsync<AddToDoApiResponse>(response);
        Assert.NotEqual(Guid.Empty.ToString(), result.Id);
        Assert.Equal("Buy groceries", result.Title);
        Assert.False(result.IsCompleted);
        Assert.Equal(FakeTimeProvider.GetUtcNow().UtcDateTime, result.CreatedAt);
        Assert.Null(result.CompletedAt);

        // Verify Location header is set correctly
        Assert.NotNull(response.Headers.Location);
        Assert.Contains(todoList.ToDoListId.ToString(), response.Headers.Location.ToString());
    }

    [Fact]
    public async Task PostTodosToList_WithEmptyTitle_ReturnsBadRequest()
    {
        // Arrange
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient);
        object request = new AddToDoRequestBuilder()
            .WithTitle("")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync($"/api/v1/todo-lists/{todoList.ToDoListId}/todos", ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostTodosToList_WithWhitespaceOnlyTitle_ReturnsBadRequest()
    {
        // Arrange
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient);
        object request = new AddToDoRequestBuilder()
            .WithTitle("   ")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync($"/api/v1/todo-lists/{todoList.ToDoListId}/todos", ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostTodosToList_WithTabOnlyTitle_ReturnsBadRequest()
    {
        // Arrange
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient);
        object request = new AddToDoRequestBuilder()
            .WithTitle("\t\t")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync($"/api/v1/todo-lists/{todoList.ToDoListId}/todos", ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostTodosToList_WithNewlineOnlyTitle_ReturnsBadRequest()
    {
        // Arrange
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient);
        object request = new AddToDoRequestBuilder()
            .WithTitle("\n\r")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync($"/api/v1/todo-lists/{todoList.ToDoListId}/todos", ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostTodosToList_WithNullTitle_ReturnsBadRequest()
    {
        // Arrange
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient);
        string requestBody = """{"title": null}""";
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync($"/api/v1/todo-lists/{todoList.ToDoListId}/todos", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostTodosToList_WithVeryLongTitle_ReturnsBadRequest()
    {
        // Arrange - Create title with 201+ characters (exceeds 200 character limit)
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient);
        string longTitle = new string('a', 201);
        object request = new AddToDoRequestBuilder()
            .WithTitle(longTitle)
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync($"/api/v1/todo-lists/{todoList.ToDoListId}/todos", ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostTodosToList_WithTitleExactly200Characters_ReturnsCreated()
    {
        // Arrange - Title at maximum boundary (200 characters)
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient);
        string maxTitle = new string('a', 200);
        object request = new AddToDoRequestBuilder()
            .WithTitle(maxTitle)
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync($"/api/v1/todo-lists/{todoList.ToDoListId}/todos", ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        AddToDoApiResponse result = await FromJsonAsync<AddToDoApiResponse>(response);
        Assert.Equal(maxTitle, result.Title);
    }

    [Fact]
    public async Task PostTodosToList_WithTitleExactly1Character_ReturnsCreated()
    {
        // Arrange - Title at minimum boundary (1 character)
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient);
        object request = new AddToDoRequestBuilder()
            .WithTitle("a")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync($"/api/v1/todo-lists/{todoList.ToDoListId}/todos", ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        AddToDoApiResponse result = await FromJsonAsync<AddToDoApiResponse>(response);
        Assert.Equal("a", result.Title);
    }

    [Fact]
    public async Task PostTodosToList_WithNonExistentListId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentListId = Guid.NewGuid();
        object request = new AddToDoRequestBuilder()
            .WithTitle("Buy groceries")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync($"/api/v1/todo-lists/{nonExistentListId}/todos", ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostTodosToList_WithInvalidGuidListId_ReturnsBadRequest()
    {
        // Arrange
        string invalidListId = "invalid-guid";
        object request = new AddToDoRequestBuilder()
            .WithTitle("Buy groceries")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync($"/api/v1/todo-lists/{invalidListId}/todos", ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostTodosToList_WithEmptyGuidListId_ReturnsBadRequest()
    {
        // Arrange
        Guid emptyGuid = Guid.Empty;
        object request = new AddToDoRequestBuilder()
            .WithTitle("Buy groceries")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync($"/api/v1/todo-lists/{emptyGuid}/todos", ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostTodosToList_WithDuplicateTitle_ReturnsBadRequest()
    {
        // Arrange
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient);
        object request = new AddToDoRequestBuilder()
            .WithTitle("Buy groceries")
            .Build();

        // Add first todo
        HttpResponseMessage firstResponse = await HttpClient.PostAsync($"/api/v1/todo-lists/{todoList.ToDoListId}/todos", ToJsonContent(request));
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        // Act - Try to add todo with same title
        HttpResponseMessage response = await HttpClient.PostAsync($"/api/v1/todo-lists/{todoList.ToDoListId}/todos", ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostTodosToList_WithMalformedJson_ReturnsBadRequest()
    {
        // Arrange
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient);
        string malformedJson = """{"title": "Buy groceries" """; // Missing closing brace
        var content = new StringContent(malformedJson, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync($"/api/v1/todo-lists/{todoList.ToDoListId}/todos", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostTodosToList_WithMissingTitleProperty_ReturnsBadRequest()
    {
        // Arrange
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient);
        string requestBody = """{}"""; // Missing title property
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync($"/api/v1/todo-lists/{todoList.ToDoListId}/todos", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostTodosToList_WithExtraProperties_ReturnsCreated()
    {
        // Arrange - JSON with extra properties should be ignored
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient);
        string requestBody = """{"title": "Buy groceries", "extraProperty": "ignored"}""";
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync($"/api/v1/todo-lists/{todoList.ToDoListId}/todos", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        AddToDoApiResponse result = await FromJsonAsync<AddToDoApiResponse>(response);
        Assert.Equal("Buy groceries", result.Title);
    }

    [Fact]
    public async Task PostTodosToList_WithTitleContainingSpecialCharacters_ReturnsCreated()
    {
        // Arrange
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient);
        object request = new AddToDoRequestBuilder()
            .WithTitle("Buy groceries! @#$%^&*()_+-={}[]|\\:;\"'<>?,./")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync($"/api/v1/todo-lists/{todoList.ToDoListId}/todos", ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        AddToDoApiResponse result = await FromJsonAsync<AddToDoApiResponse>(response);
        Assert.Equal("Buy groceries! @#$%^&*()_+-={}[]|\\:;\"'<>?,./", result.Title);
    }

    [Fact]
    public async Task PostTodosToList_WithTitleContainingUnicodeCharacters_ReturnsCreated()
    {
        // Arrange
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient);
        object request = new AddToDoRequestBuilder()
            .WithTitle("買い物 🛒 épicerie مشتريات")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync($"/api/v1/todo-lists/{todoList.ToDoListId}/todos", ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        AddToDoApiResponse result = await FromJsonAsync<AddToDoApiResponse>(response);
        Assert.Equal("買い物 🛒 épicerie مشتريات", result.Title);
    }

    [Fact]
    public async Task PostTodosToList_WithTitleContainingNewlines_ReturnsCreated()
    {
        // Arrange - Newlines within the title should be accepted
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient);
        object request = new AddToDoRequestBuilder()
            .WithTitle("Buy:\n- Milk\n- Bread\n- Eggs")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync($"/api/v1/todo-lists/{todoList.ToDoListId}/todos", ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        AddToDoApiResponse result = await FromJsonAsync<AddToDoApiResponse>(response);
        Assert.Equal("Buy:\n- Milk\n- Bread\n- Eggs", result.Title);
    }

    [Fact]
    public async Task PostTodosToList_WithWrongContentType_ReturnsUnsupportedMediaType()
    {
        // Arrange
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient);
        object request = new AddToDoRequestBuilder().Build();
        string json = System.Text.Json.JsonSerializer.Serialize(request, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
        var content = new StringContent(json, Encoding.UTF8, "application/xml");

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync($"/api/v1/todo-lists/{todoList.ToDoListId}/todos", content);

        // Assert
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
    }

    [Fact]
    public async Task PostTodosToList_WithMissingContentType_ReturnsBadRequest()
    {
        // Arrange
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient);
        object request = new AddToDoRequestBuilder().Build();
        string json = System.Text.Json.JsonSerializer.Serialize(request, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
        var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = null;

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync($"/api/v1/todo-lists/{todoList.ToDoListId}/todos", content);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.UnsupportedMediaType);
    }

    [Fact]
    public async Task PostTodosToList_MultipleValidTodos_ReturnsCreatedForEach()
    {
        // Arrange
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient);
        string[] todoTitles = { "Buy milk", "Walk the dog", "Write tests", "Deploy to production" };

        // Act & Assert - Add multiple todos
        for (int i = 0; i < todoTitles.Length; i++)
        {
            object request = new AddToDoRequestBuilder()
                .WithTitle(todoTitles[i])
                .Build();

            HttpResponseMessage response = await HttpClient.PostAsync($"/api/v1/todo-lists/{todoList.ToDoListId}/todos", ToJsonContent(request));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            AddToDoApiResponse result = await FromJsonAsync<AddToDoApiResponse>(response);
            Assert.Equal(todoTitles[i], result.Title);
            Assert.NotEqual(Guid.Empty.ToString(), result.Id);
        }
    }

    [Fact]
    public async Task PostTodosToList_WhenListHas100Todos_ReturnsBadRequest()
    {
        // Arrange - Create a todo list and add 100 todos (the maximum)
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient);

        for (int i = 1; i <= 100; i++)
        {
            object request = new AddToDoRequestBuilder()
                .WithTitle($"Todo item {i}")
                .Build();
            HttpResponseMessage response = await HttpClient.PostAsync($"/api/v1/todo-lists/{todoList.ToDoListId}/todos", ToJsonContent(request));
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        // Act - Try to add the 101st todo
        object exceededRequest = new AddToDoRequestBuilder()
            .WithTitle("Todo item 101")
            .Build();
        HttpResponseMessage exceededResponse = await HttpClient.PostAsync($"/api/v1/todo-lists/{todoList.ToDoListId}/todos", ToJsonContent(exceededRequest));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, exceededResponse.StatusCode);
    }

    [Fact]
    public async Task PutTodos_OnPostTodosEndpoint_ReturnsMethodNotAllowed()
    {
        // Arrange
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient);
        object request = new AddToDoRequestBuilder().Build();

        // Act
        HttpResponseMessage response = await HttpClient.PutAsync($"/api/v1/todo-lists/{todoList.ToDoListId}/todos", ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task GetTodos_OnPostTodosEndpoint_ReturnsMethodNotAllowed()
    {
        // Arrange
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient);

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists/{todoList.ToDoListId}/todos");

        // Assert
        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTodos_OnPostTodosEndpoint_ReturnsMethodNotAllowed()
    {
        // Arrange
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient);

        // Act
        HttpResponseMessage response = await HttpClient.DeleteAsync($"/api/v1/todo-lists/{todoList.ToDoListId}/todos");

        // Assert
        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task PostTodosToList_WithMalformedJsonMissingQuotes_ReturnsBadRequest()
    {
        // Arrange
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient);
        string malformedJson = """{title: "Buy groceries"}"""; // Missing quotes on property name
        var content = new StringContent(malformedJson, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync($"/api/v1/todo-lists/{todoList.ToDoListId}/todos", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostTodosToList_WithMalformedJsonExtraComma_ReturnsBadRequest()
    {
        // Arrange
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient);
        string malformedJson = """{"title": "Buy groceries",}"""; // Trailing comma
        var content = new StringContent(malformedJson, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync($"/api/v1/todo-lists/{todoList.ToDoListId}/todos", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostTodosToList_WithInvalidJsonStructure_ReturnsBadRequest()
    {
        // Arrange
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient);
        string invalidJson = """["title"]"""; // JSON array instead of object
        var content = new StringContent(invalidJson, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync($"/api/v1/todo-lists/{todoList.ToDoListId}/todos", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
