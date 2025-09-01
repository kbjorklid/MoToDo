using System.Net;
using SystemTests.TestObjectBuilders;
using ToDoLists.Contracts;

namespace SystemTests.ToDoLists;

/// <summary>
/// System tests for ToDoLists GET endpoints.
/// </summary>
public class ToDoListsControllerGetTests : BaseSystemTest, IAsyncLifetime
{
    public ToDoListsControllerGetTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetToDoList_WithValidIdAndUserId_ReturnsOkWithToDoListDetail()
    {
        // Arrange
        CreateToDoListResult createdList = await ToDoListsTestHelper.CreateToDoListAsync(HttpClient, "My Shopping List");
        string userId = createdList.UserId.ToString();
        string listId = createdList.ToDoListId.ToString();

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists/{listId}?userId={userId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ToDoListDetailDto result = await FromJsonAsync<ToDoListDetailDto>(response);
        Assert.Equal(createdList.ToDoListId, result.Id);
        Assert.Equal("My Shopping List", result.Title);
        Assert.Empty(result.Todos);
        Assert.Equal(0, result.TodoCount);
        Assert.Equal(FakeTimeProvider.GetUtcNow().UtcDateTime, result.CreatedAt);
        Assert.Null(result.UpdatedAt);
    }

    [Fact]
    public async Task GetToDoList_WithToDoListContainingMultipleTodos_ReturnsOkWithAllTodos()
    {
        // Arrange
        CreateToDoListResult createdList = await ToDoListsTestHelper.CreateToDoListAsync(HttpClient, "Shopping List");
        string userId = createdList.UserId.ToString();
        string listId = createdList.ToDoListId.ToString();

        // Add multiple todos to the list
        object addTodoRequest1 = new AddToDoRequestBuilder().WithTitle("Buy milk").Build();
        object addTodoRequest2 = new AddToDoRequestBuilder().WithTitle("Buy bread").Build();

        await HttpClient.PostAsync($"/api/v1/todo-lists/{listId}/todos", ToJsonContent(addTodoRequest1));
        await HttpClient.PostAsync($"/api/v1/todo-lists/{listId}/todos", ToJsonContent(addTodoRequest2));

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists/{listId}?userId={userId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ToDoListDetailDto result = await FromJsonAsync<ToDoListDetailDto>(response);
        Assert.Equal(2, result.TodoCount);
        Assert.Equal(2, result.Todos.Length);

        // Verify todos content
        Assert.Contains(result.Todos, t => t.Title == "Buy milk" && !t.IsCompleted);
        Assert.Contains(result.Todos, t => t.Title == "Buy bread" && !t.IsCompleted);

        // Verify all todos have valid creation timestamps
        Assert.All(result.Todos, todo =>
        {
            Assert.NotEqual(Guid.Empty, todo.Id);
            Assert.Equal(FakeTimeProvider.GetUtcNow().UtcDateTime, todo.CreatedAt);
            Assert.Null(todo.CompletedAt);
        });
    }

    [Fact]
    public async Task GetToDoList_WithEmptyToDoList_ReturnsOkWithEmptyTodosArray()
    {
        // Arrange
        CreateToDoListResult createdList = await ToDoListsTestHelper.CreateToDoListAsync(HttpClient);
        string userId = createdList.UserId.ToString();
        string listId = createdList.ToDoListId.ToString();

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists/{listId}?userId={userId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ToDoListDetailDto result = await FromJsonAsync<ToDoListDetailDto>(response);
        Assert.Empty(result.Todos);
        Assert.Equal(0, result.TodoCount);
    }

    [Fact]
    public async Task GetToDoList_WithInvalidToDoListIdFormat_ReturnsBadRequest()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists/invalid-guid-format?userId={userId}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetToDoList_WithEmptyToDoListId_ReturnsBadRequest()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();

        // Act - Using empty GUID string
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists/00000000-0000-0000-0000-000000000000?userId={userId}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetToDoList_WithInvalidUserIdFormat_ReturnsBadRequest()
    {
        // Arrange
        string listId = Guid.NewGuid().ToString();

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists/{listId}?userId=invalid-guid-format");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetToDoList_WithEmptyUserId_ReturnsBadRequest()
    {
        // Arrange
        string listId = Guid.NewGuid().ToString();

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists/{listId}?userId=");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetToDoList_WithMissingUserIdQueryParameter_ReturnsBadRequest()
    {
        // Arrange
        string listId = Guid.NewGuid().ToString();

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists/{listId}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetToDoList_WithValidIdsBuUserDoesNotOwnList_ReturnsForbidden()
    {
        // Arrange
        CreateToDoListResult createdList = await ToDoListsTestHelper.CreateToDoListAsync(HttpClient);
        string listId = createdList.ToDoListId.ToString();
        string differentUserId = Guid.NewGuid().ToString(); // Different user ID

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists/{listId}?userId={differentUserId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetToDoList_WithValidIdsButNonExistentList_ReturnsNotFound()
    {
        // Arrange
        string nonExistentListId = Guid.NewGuid().ToString();
        string userId = Guid.NewGuid().ToString();

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists/{nonExistentListId}?userId={userId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostToDoList_OnGetEndpoint_ReturnsMethodNotAllowed()
    {
        // Arrange
        CreateToDoListCommand command = new CreateToDoListCommandBuilder().Build();
        string listId = Guid.NewGuid().ToString();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync($"/api/v1/todo-lists/{listId}", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }
}
