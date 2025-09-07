using System.Net;
using System.Text;
using SystemTests.TestObjectBuilders;
using SystemTests.Users;
using ToDoLists.Contracts;

namespace SystemTests.ToDoLists;

/// <summary>
/// System tests for PUT /todo-lists/{listId}/todos/{todoId} endpoint.
/// </summary>
public class ToDoListControllerPutTodosTests : BaseSystemTest
{
    public ToDoListControllerPutTodosTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }


    [Fact]
    public async Task PutTodos_WithValidTitleUpdate_ReturnsOkWithUpdatedTodo()
    {
        // Arrange
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, userId, "Shopping List");
        AddToDoResult todo = await ToDoListTestHelper.AddToDoAsync(HttpClient, todoList.ToDoListId, "Buy milk");

        object request = new UpdateToDoRequestBuilder()
            .WithTitle("Buy organic milk")
            .WithNoIsCompleted()
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PutAsync(
            $"/api/v1/todo-lists/{todoList.ToDoListId}/todos/{todo.Id}?userId={userId}",
            ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        UpdateToDoApiResponse result = await FromJsonAsync<UpdateToDoApiResponse>(response);
        Assert.Equal(todo.Id.ToString(), result.Id);
        Assert.Equal("Buy organic milk", result.Title);
        Assert.False(result.IsCompleted);
        Assert.Equal(todo.CreatedAt, result.CreatedAt);
        Assert.Null(result.CompletedAt);
    }

    [Fact]
    public async Task PutTodos_WithValidCompletionUpdate_ReturnsOkWithCompletedTodo()
    {
        // Arrange
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, userId, "Shopping List");
        AddToDoResult todo = await ToDoListTestHelper.AddToDoAsync(HttpClient, todoList.ToDoListId, "Buy milk");

        object request = new UpdateToDoRequestBuilder()
            .WithNoTitle()
            .WithIsCompleted(true)
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PutAsync(
            $"/api/v1/todo-lists/{todoList.ToDoListId}/todos/{todo.Id}?userId={userId}",
            ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        UpdateToDoApiResponse result = await FromJsonAsync<UpdateToDoApiResponse>(response);
        Assert.Equal(todo.Id.ToString(), result.Id);
        Assert.Equal("Buy milk", result.Title); // Title unchanged
        Assert.True(result.IsCompleted);
        Assert.Equal(todo.CreatedAt, result.CreatedAt);
        Assert.Equal(FakeTimeProvider.GetUtcNow().UtcDateTime, result.CompletedAt);
    }

    [Fact]
    public async Task PutTodos_WithBothTitleAndCompletionUpdate_ReturnsOkWithUpdatedTodo()
    {
        // Arrange
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, userId, "Shopping List");
        AddToDoResult todo = await ToDoListTestHelper.AddToDoAsync(HttpClient, todoList.ToDoListId, "Buy milk");

        object request = new UpdateToDoRequestBuilder()
            .WithTitle("Buy organic milk")
            .WithIsCompleted(true)
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PutAsync(
            $"/api/v1/todo-lists/{todoList.ToDoListId}/todos/{todo.Id}?userId={userId}",
            ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        UpdateToDoApiResponse result = await FromJsonAsync<UpdateToDoApiResponse>(response);
        Assert.Equal(todo.Id.ToString(), result.Id);
        Assert.Equal("Buy organic milk", result.Title);
        Assert.True(result.IsCompleted);
        Assert.Equal(todo.CreatedAt, result.CreatedAt);
        Assert.Equal(FakeTimeProvider.GetUtcNow().UtcDateTime, result.CompletedAt);
    }

    [Fact]
    public async Task PutTodos_WithMarkingCompletedTodoAsIncomplete_ReturnsOkWithIncompleteTodo()
    {
        // Arrange
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, userId, "Shopping List");
        AddToDoResult todo = await ToDoListTestHelper.AddToDoAsync(HttpClient, todoList.ToDoListId, "Buy milk");

        // First mark as completed
        object completeRequest = new UpdateToDoRequestBuilder()
            .WithNoTitle()
            .WithIsCompleted(true)
            .Build();
        await HttpClient.PutAsync(
            $"/api/v1/todo-lists/{todoList.ToDoListId}/todos/{todo.Id}?userId={userId}",
            ToJsonContent(completeRequest));

        // Now mark as incomplete
        object incompleteRequest = new UpdateToDoRequestBuilder()
            .WithNoTitle()
            .WithIsCompleted(false)
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PutAsync(
            $"/api/v1/todo-lists/{todoList.ToDoListId}/todos/{todo.Id}?userId={userId}",
            ToJsonContent(incompleteRequest));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        UpdateToDoApiResponse result = await FromJsonAsync<UpdateToDoApiResponse>(response);
        Assert.Equal(todo.Id.ToString(), result.Id);
        Assert.Equal("Buy milk", result.Title);
        Assert.False(result.IsCompleted);
        Assert.Equal(todo.CreatedAt, result.CreatedAt);
        Assert.Null(result.CompletedAt);
    }

    [Fact]
    public async Task PutTodos_WithEmptyTitleAndNoCompletion_ReturnsBadRequest()
    {
        // Arrange
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, userId, "Shopping List");
        AddToDoResult todo = await ToDoListTestHelper.AddToDoAsync(HttpClient, todoList.ToDoListId, "Buy milk");

        object request = new UpdateToDoRequestBuilder()
            .WithTitle("")
            .WithNoIsCompleted()
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PutAsync(
            $"/api/v1/todo-lists/{todoList.ToDoListId}/todos/{todo.Id}?userId={userId}",
            ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PutTodos_WithWhitespaceOnlyTitle_ReturnsBadRequest()
    {
        // Arrange
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, userId, "Shopping List");
        AddToDoResult todo = await ToDoListTestHelper.AddToDoAsync(HttpClient, todoList.ToDoListId, "Buy milk");

        object request = new UpdateToDoRequestBuilder()
            .WithTitle("   ")
            .WithNoIsCompleted()
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PutAsync(
            $"/api/v1/todo-lists/{todoList.ToDoListId}/todos/{todo.Id}?userId={userId}",
            ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PutTodos_WithVeryLongTitle_ReturnsBadRequest()
    {
        // Arrange - Create title with 201+ characters (exceeds 200 character limit)
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, userId, "Shopping List");
        AddToDoResult todo = await ToDoListTestHelper.AddToDoAsync(HttpClient, todoList.ToDoListId, "Buy milk");

        string longTitle = new string('a', 201);
        object request = new UpdateToDoRequestBuilder()
            .WithTitle(longTitle)
            .WithNoIsCompleted()
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PutAsync(
            $"/api/v1/todo-lists/{todoList.ToDoListId}/todos/{todo.Id}?userId={userId}",
            ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PutTodos_WithTitleExactly200Characters_ReturnsOk()
    {
        // Arrange - Title at maximum boundary (200 characters)
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, userId, "Shopping List");
        AddToDoResult todo = await ToDoListTestHelper.AddToDoAsync(HttpClient, todoList.ToDoListId, "Buy milk");

        string maxTitle = new string('b', 200);
        object request = new UpdateToDoRequestBuilder()
            .WithTitle(maxTitle)
            .WithNoIsCompleted()
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PutAsync(
            $"/api/v1/todo-lists/{todoList.ToDoListId}/todos/{todo.Id}?userId={userId}",
            ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        UpdateToDoApiResponse result = await FromJsonAsync<UpdateToDoApiResponse>(response);
        Assert.Equal(maxTitle, result.Title);
    }

    [Fact]
    public async Task PutTodos_WithTitleExactly1Character_ReturnsOk()
    {
        // Arrange - Title at minimum boundary (1 character)
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, userId, "Shopping List");
        AddToDoResult todo = await ToDoListTestHelper.AddToDoAsync(HttpClient, todoList.ToDoListId, "Buy milk");

        object request = new UpdateToDoRequestBuilder()
            .WithTitle("x")
            .WithNoIsCompleted()
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PutAsync(
            $"/api/v1/todo-lists/{todoList.ToDoListId}/todos/{todo.Id}?userId={userId}",
            ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        UpdateToDoApiResponse result = await FromJsonAsync<UpdateToDoApiResponse>(response);
        Assert.Equal("x", result.Title);
    }

    [Fact]
    public async Task PutTodos_WithDuplicateTitleInSameList_ReturnsBadRequest()
    {
        // Arrange
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, userId, "Shopping List");
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, todoList.ToDoListId, "Buy milk");
        AddToDoResult secondTodo = await ToDoListTestHelper.AddToDoAsync(HttpClient, todoList.ToDoListId, "Buy bread");

        object request = new UpdateToDoRequestBuilder()
            .WithTitle("Buy milk") // Same title as first todo
            .WithNoIsCompleted()
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PutAsync(
            $"/api/v1/todo-lists/{todoList.ToDoListId}/todos/{secondTodo.Id}?userId={userId}",
            ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PutTodos_WithSameTitleAsCurrentTodo_ReturnsOk()
    {
        // Arrange - Updating a todo with its current title should be allowed
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, userId, "Shopping List");
        AddToDoResult todo = await ToDoListTestHelper.AddToDoAsync(HttpClient, todoList.ToDoListId, "Buy milk");

        object request = new UpdateToDoRequestBuilder()
            .WithTitle("Buy milk") // Same as current title
            .WithIsCompleted(true)
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PutAsync(
            $"/api/v1/todo-lists/{todoList.ToDoListId}/todos/{todo.Id}?userId={userId}",
            ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        UpdateToDoApiResponse result = await FromJsonAsync<UpdateToDoApiResponse>(response);
        Assert.Equal("Buy milk", result.Title);
        Assert.True(result.IsCompleted);
    }

    [Fact]
    public async Task PutTodos_WithNonExistentTodoId_ReturnsNotFound()
    {
        // Arrange
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, userId, "Shopping List");
        var nonExistentTodoId = Guid.NewGuid();

        object request = new UpdateToDoRequestBuilder()
            .WithTitle("Updated title")
            .WithNoIsCompleted()
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PutAsync(
            $"/api/v1/todo-lists/{todoList.ToDoListId}/todos/{nonExistentTodoId}?userId={userId}",
            ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PutTodos_WithNonExistentListId_ReturnsNotFound()
    {
        // Arrange
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);
        var nonExistentListId = Guid.NewGuid();
        var todoId = Guid.NewGuid();

        object request = new UpdateToDoRequestBuilder()
            .WithTitle("Updated title")
            .WithNoIsCompleted()
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PutAsync(
            $"/api/v1/todo-lists/{nonExistentListId}/todos/{todoId}?userId={userId}",
            ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PutTodos_WithInvalidGuidListId_ReturnsBadRequest()
    {
        // Arrange
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);
        string invalidListId = "invalid-guid";
        var todoId = Guid.NewGuid();

        object request = new UpdateToDoRequestBuilder()
            .WithTitle("Updated title")
            .WithNoIsCompleted()
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PutAsync(
            $"/api/v1/todo-lists/{invalidListId}/todos/{todoId}?userId={userId}",
            ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PutTodos_WithInvalidGuidTodoId_ReturnsBadRequest()
    {
        // Arrange
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, userId, "Shopping List");
        string invalidTodoId = "invalid-guid";

        object request = new UpdateToDoRequestBuilder()
            .WithTitle("Updated title")
            .WithNoIsCompleted()
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PutAsync(
            $"/api/v1/todo-lists/{todoList.ToDoListId}/todos/{invalidTodoId}?userId={userId}",
            ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PutTodos_WithEmptyGuidListId_ReturnsBadRequest()
    {
        // Arrange
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);
        Guid emptyListId = Guid.Empty;
        var todoId = Guid.NewGuid();

        object request = new UpdateToDoRequestBuilder()
            .WithTitle("Updated title")
            .WithNoIsCompleted()
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PutAsync(
            $"/api/v1/todo-lists/{emptyListId}/todos/{todoId}?userId={userId}",
            ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PutTodos_WithEmptyGuidTodoId_ReturnsBadRequest()
    {
        // Arrange
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, userId, "Shopping List");
        Guid emptyTodoId = Guid.Empty;

        object request = new UpdateToDoRequestBuilder()
            .WithTitle("Updated title")
            .WithNoIsCompleted()
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PutAsync(
            $"/api/v1/todo-lists/{todoList.ToDoListId}/todos/{emptyTodoId}?userId={userId}",
            ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PutTodos_WithMissingUserId_ReturnsBadRequest()
    {
        // Arrange
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, userId, "Shopping List");
        AddToDoResult todo = await ToDoListTestHelper.AddToDoAsync(HttpClient, todoList.ToDoListId, "Buy milk");

        object request = new UpdateToDoRequestBuilder()
            .WithTitle("Updated title")
            .WithNoIsCompleted()
            .Build();

        // Act - Missing userId query parameter
        HttpResponseMessage response = await HttpClient.PutAsync(
            $"/api/v1/todo-lists/{todoList.ToDoListId}/todos/{todo.Id}",
            ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PutTodos_WithInvalidUserId_ReturnsBadRequest()
    {
        // Arrange
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient);
        AddToDoResult todo = await ToDoListTestHelper.AddToDoAsync(HttpClient, todoList.ToDoListId, "Buy milk");

        object request = new UpdateToDoRequestBuilder()
            .WithTitle("Updated title")
            .WithNoIsCompleted()
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PutAsync(
            $"/api/v1/todo-lists/{todoList.ToDoListId}/todos/{todo.Id}?userId=invalid-guid",
            ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PutTodos_WithUnauthorizedUser_ReturnsForbidden()
    {
        // Arrange - Create todo list with one user, try to update with different user
        Guid user1Id = await UserTestHelper.CreateUserAsync(HttpClient);
        Guid user2Id = await UserTestHelper.CreateUserAsync(HttpClient);
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, user1Id, "Shopping List");
        AddToDoResult todo = await ToDoListTestHelper.AddToDoAsync(HttpClient, todoList.ToDoListId, "Buy milk");

        object request = new UpdateToDoRequestBuilder()
            .WithTitle("Unauthorized update")
            .WithNoIsCompleted()
            .Build();

        // Act - Try to update with different user
        HttpResponseMessage response = await HttpClient.PutAsync(
            $"/api/v1/todo-lists/{todoList.ToDoListId}/todos/{todo.Id}?userId={user2Id}",
            ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PutTodos_WithBothTitleAndCompletionNull_ReturnsOk()
    {
        // Arrange - Request with both null values should succeed as no-op
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, userId, "Shopping List");
        AddToDoResult todo = await ToDoListTestHelper.AddToDoAsync(HttpClient, todoList.ToDoListId, "Buy milk");

        object request = new UpdateToDoRequestBuilder()
            .WithNoTitle()
            .WithNoIsCompleted()
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PutAsync(
            $"/api/v1/todo-lists/{todoList.ToDoListId}/todos/{todo.Id}?userId={userId}",
            ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        UpdateToDoApiResponse result = await FromJsonAsync<UpdateToDoApiResponse>(response);
        Assert.Equal(todo.Id.ToString(), result.Id);
        Assert.Equal("Buy milk", result.Title); // Unchanged
        Assert.False(result.IsCompleted); // Unchanged
    }

    [Fact]
    public async Task PutTodos_WithMalformedJson_ReturnsBadRequest()
    {
        // Arrange
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, userId, "Shopping List");
        AddToDoResult todo = await ToDoListTestHelper.AddToDoAsync(HttpClient, todoList.ToDoListId, "Buy milk");

        string malformedJson = """{"title": "Updated title" """; // Missing closing brace
        var content = new StringContent(malformedJson, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await HttpClient.PutAsync(
            $"/api/v1/todo-lists/{todoList.ToDoListId}/todos/{todo.Id}?userId={userId}",
            content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PutTodos_WithEmptyJsonBody_ReturnsOk()
    {
        // Arrange - Empty JSON body should be treated as no updates
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, userId, "Shopping List");
        AddToDoResult todo = await ToDoListTestHelper.AddToDoAsync(HttpClient, todoList.ToDoListId, "Buy milk");

        string emptyJson = "{}";
        var content = new StringContent(emptyJson, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await HttpClient.PutAsync(
            $"/api/v1/todo-lists/{todoList.ToDoListId}/todos/{todo.Id}?userId={userId}",
            content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        UpdateToDoApiResponse result = await FromJsonAsync<UpdateToDoApiResponse>(response);
        Assert.Equal("Buy milk", result.Title); // Unchanged
        Assert.False(result.IsCompleted); // Unchanged
    }

    [Fact]
    public async Task PutTodos_WithTitleContainingSpecialCharacters_ReturnsOk()
    {
        // Arrange
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, userId, "Shopping List");
        AddToDoResult todo = await ToDoListTestHelper.AddToDoAsync(HttpClient, todoList.ToDoListId, "Buy milk");

        object request = new UpdateToDoRequestBuilder()
            .WithTitle("Buy groceries! @#$%^&*()_+-={}[]|\\:;\"'<>?,./")
            .WithNoIsCompleted()
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PutAsync(
            $"/api/v1/todo-lists/{todoList.ToDoListId}/todos/{todo.Id}?userId={userId}",
            ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        UpdateToDoApiResponse result = await FromJsonAsync<UpdateToDoApiResponse>(response);
        Assert.Equal("Buy groceries! @#$%^&*()_+-={}[]|\\:;\"'<>?,./", result.Title);
    }

    [Fact]
    public async Task PutTodos_WithTitleContainingUnicodeCharacters_ReturnsOk()
    {
        // Arrange
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, userId, "Shopping List");
        AddToDoResult todo = await ToDoListTestHelper.AddToDoAsync(HttpClient, todoList.ToDoListId, "Buy milk");

        object request = new UpdateToDoRequestBuilder()
            .WithTitle("買い物 🛒 épicerie مشتريات")
            .WithNoIsCompleted()
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PutAsync(
            $"/api/v1/todo-lists/{todoList.ToDoListId}/todos/{todo.Id}?userId={userId}",
            ToJsonContent(request));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        UpdateToDoApiResponse result = await FromJsonAsync<UpdateToDoApiResponse>(response);
        Assert.Equal("買い物 🛒 épicerie مشتريات", result.Title);
    }
}
