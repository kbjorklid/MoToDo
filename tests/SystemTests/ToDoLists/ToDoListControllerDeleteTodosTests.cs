using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SystemTests.Users;
using ToDoLists.Contracts;
using ToDoLists.Infrastructure;

namespace SystemTests.ToDoLists;

/// <summary>
/// System tests for individual todo item DELETE endpoints in ToDoLists.
/// </summary>
public class ToDoListControllerDeleteTodosTests : BaseSystemTest
{
    public ToDoListControllerDeleteTodosTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public async Task DeleteToDoItem_WithValidIds_ReturnsNoContent()
    {
        // Arrange
        CreateToDoListResult createdList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, "Shopping List");
        AddToDoResult createdTodo = await ToDoListTestHelper.AddToDoAsync(HttpClient, createdList.ToDoListId, "Buy milk");
        string userId = createdList.UserId.ToString();
        string listId = createdList.ToDoListId.ToString();
        string todoId = createdTodo.Id.ToString();

        // Act
        HttpResponseMessage response = await HttpClient.DeleteAsync($"/api/v1/todo-lists/{listId}/todos/{todoId}?userId={userId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteToDoItem_WithValidIds_RemovesToDoFromDatabase()
    {
        // Arrange
        CreateToDoListResult createdList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, "Shopping List");
        AddToDoResult createdTodo = await ToDoListTestHelper.AddToDoAsync(HttpClient, createdList.ToDoListId, "Buy milk");
        string userId = createdList.UserId.ToString();
        string listId = createdList.ToDoListId.ToString();
        string todoId = createdTodo.Id.ToString();

        // Act
        HttpResponseMessage response = await HttpClient.DeleteAsync($"/api/v1/todo-lists/{listId}/todos/{todoId}?userId={userId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify todo item is actually removed from database
        using IServiceScope scope = WebAppFactory.Services.CreateScope();
        ToDoListsDbContext dbContext = scope.ServiceProvider.GetRequiredService<ToDoListsDbContext>();

        // The todo list should still exist but without the removed todo
        global::ToDoLists.Domain.ToDoList? todoListFromDb = await dbContext.ToDoLists
            .Include(tl => tl.Todos)
            .FirstOrDefaultAsync(tl => tl.Id == createdList.ToDoListId);

        Assert.NotNull(todoListFromDb);
        Assert.DoesNotContain(todoListFromDb.Todos, t => t.Id == createdTodo.Id);
    }

    [Fact]
    public async Task DeleteToDoItem_FromListWithMultipleTodos_OnlyRemovesSpecifiedTodo()
    {
        // Arrange
        CreateToDoListResult createdList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, "Shopping List");
        AddToDoResult todoToDelete = await ToDoListTestHelper.AddToDoAsync(HttpClient, createdList.ToDoListId, "Buy milk");
        AddToDoResult todoToKeep = await ToDoListTestHelper.AddToDoAsync(HttpClient, createdList.ToDoListId, "Buy bread");
        string userId = createdList.UserId.ToString();
        string listId = createdList.ToDoListId.ToString();
        string todoId = todoToDelete.Id.ToString();

        // Act
        HttpResponseMessage response = await HttpClient.DeleteAsync($"/api/v1/todo-lists/{listId}/todos/{todoId}?userId={userId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify only the specified todo is removed
        using IServiceScope scope = WebAppFactory.Services.CreateScope();
        ToDoListsDbContext dbContext = scope.ServiceProvider.GetRequiredService<ToDoListsDbContext>();

        global::ToDoLists.Domain.ToDoList? todoListFromDb = await dbContext.ToDoLists
            .Include(tl => tl.Todos)
            .FirstOrDefaultAsync(tl => tl.Id == createdList.ToDoListId);

        Assert.NotNull(todoListFromDb);
        Assert.DoesNotContain(todoListFromDb.Todos, t => t.Id == todoToDelete.Id);
        Assert.Contains(todoListFromDb.Todos, t => t.Id == todoToKeep.Id);
    }

    [Theory]
    [InlineData("invalid-guid")]
    [InlineData("12345")]
    [InlineData("not-a-guid-at-all")]
    public async Task DeleteToDoItem_WithInvalidToDoListIdFormat_ReturnsBadRequest(string invalidListId)
    {
        // Arrange
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);
        string validTodoId = Guid.NewGuid().ToString();

        // Act
        HttpResponseMessage response = await HttpClient.DeleteAsync($"/api/v1/todo-lists/{invalidListId}/todos/{validTodoId}?userId={userId}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteToDoItem_WithEmptyToDoListId_ReturnsNotFound()
    {
        // Arrange
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);
        string validTodoId = Guid.NewGuid().ToString();

        // Act - Empty string in route parameter results in route mismatch
        HttpResponseMessage response = await HttpClient.DeleteAsync($"/api/v1/todo-lists//todos/{validTodoId}?userId={userId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("invalid-guid")]
    [InlineData("12345")]
    [InlineData("not-a-guid-at-all")]
    public async Task DeleteToDoItem_WithInvalidToDoIdFormat_ReturnsBadRequest(string invalidTodoId)
    {
        // Arrange
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);
        string validListId = Guid.NewGuid().ToString();

        // Act
        HttpResponseMessage response = await HttpClient.DeleteAsync($"/api/v1/todo-lists/{validListId}/todos/{invalidTodoId}?userId={userId}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteToDoItem_WithEmptyToDoId_ReturnsMethodNotAllowed()
    {
        // Arrange
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);
        string validListId = Guid.NewGuid().ToString();

        // Act - Empty string in route parameter results in route mismatch
        HttpResponseMessage response = await HttpClient.DeleteAsync($"/api/v1/todo-lists/{validListId}/todos/?userId={userId}");

        // Assert
        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-guid")]
    [InlineData("12345")]
    [InlineData("not-a-guid-at-all")]
    public async Task DeleteToDoItem_WithInvalidUserIdFormat_ReturnsBadRequest(string invalidUserId)
    {
        // Arrange
        string validListId = Guid.NewGuid().ToString();
        string validTodoId = Guid.NewGuid().ToString();

        // Act
        HttpResponseMessage response = await HttpClient.DeleteAsync($"/api/v1/todo-lists/{validListId}/todos/{validTodoId}?userId={invalidUserId}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteToDoItem_WithMissingUserIdQueryParameter_ReturnsBadRequest()
    {
        // Arrange
        string validListId = Guid.NewGuid().ToString();
        string validTodoId = Guid.NewGuid().ToString();

        // Act
        HttpResponseMessage response = await HttpClient.DeleteAsync($"/api/v1/todo-lists/{validListId}/todos/{validTodoId}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteToDoItem_WithNonExistentToDoList_ReturnsNotFound()
    {
        // Arrange
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);
        string nonExistentListId = Guid.NewGuid().ToString();
        string validTodoId = Guid.NewGuid().ToString();

        // Act
        HttpResponseMessage response = await HttpClient.DeleteAsync($"/api/v1/todo-lists/{nonExistentListId}/todos/{validTodoId}?userId={userId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteToDoItem_WithNonExistentToDo_ReturnsNotFound()
    {
        // Arrange
        CreateToDoListResult createdList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, "Shopping List");
        string userId = createdList.UserId.ToString();
        string listId = createdList.ToDoListId.ToString();
        string nonExistentTodoId = Guid.NewGuid().ToString();

        // Act
        HttpResponseMessage response = await HttpClient.DeleteAsync($"/api/v1/todo-lists/{listId}/todos/{nonExistentTodoId}?userId={userId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteToDoItem_WithUserWhoDoesNotOwnList_ReturnsForbidden()
    {
        // Arrange
        CreateToDoListResult createdList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, "Shopping List");
        AddToDoResult createdTodo = await ToDoListTestHelper.AddToDoAsync(HttpClient, createdList.ToDoListId, "Buy milk");

        // Create a different user who doesn't own the list
        Guid differentUserId = await UserTestHelper.CreateUserAsync(HttpClient);

        string listId = createdList.ToDoListId.ToString();
        string todoId = createdTodo.Id.ToString();

        // Act
        HttpResponseMessage response = await HttpClient.DeleteAsync($"/api/v1/todo-lists/{listId}/todos/{todoId}?userId={differentUserId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteToDoItem_WithValidIds_DoesNotAffectOtherUsersLists()
    {
        // Arrange
        // User 1 creates a list and todo
        CreateToDoListResult user1List = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, "User 1 List");
        AddToDoResult user1Todo = await ToDoListTestHelper.AddToDoAsync(HttpClient, user1List.ToDoListId, "User 1 Todo");

        // User 2 creates a list and todo
        Guid user2Id = await UserTestHelper.CreateUserAsync(HttpClient);
        CreateToDoListResult user2List = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, user2Id, "User 2 List");
        AddToDoResult user2Todo = await ToDoListTestHelper.AddToDoAsync(HttpClient, user2List.ToDoListId, "User 2 Todo");

        // Act - User 1 deletes their own todo
        string userId1 = user1List.UserId.ToString();
        string listId1 = user1List.ToDoListId.ToString();
        string todoId1 = user1Todo.Id.ToString();

        HttpResponseMessage response = await HttpClient.DeleteAsync($"/api/v1/todo-lists/{listId1}/todos/{todoId1}?userId={userId1}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify User 2's todo is still present
        using IServiceScope scope = WebAppFactory.Services.CreateScope();
        ToDoListsDbContext dbContext = scope.ServiceProvider.GetRequiredService<ToDoListsDbContext>();

        global::ToDoLists.Domain.ToDoList? user2ListFromDb = await dbContext.ToDoLists
            .Include(tl => tl.Todos)
            .FirstOrDefaultAsync(tl => tl.Id == user2List.ToDoListId);

        Assert.NotNull(user2ListFromDb);
        Assert.Contains(user2ListFromDb.Todos, t => t.Id == user2Todo.Id);
    }

    [Fact]
    public async Task PostTodos_OnDeleteTodosEndpoint_ReturnsMethodNotAllowed()
    {
        // Arrange
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);
        string listId = Guid.NewGuid().ToString();
        string todoId = Guid.NewGuid().ToString();

        // Act - POST to individual todo URL should return method not allowed  
        HttpResponseMessage response = await HttpClient.PostAsync($"/api/v1/todo-lists/{listId}/todos/{todoId}?userId={userId}", null);

        // Assert
        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task GetTodos_OnDeleteTodosEndpoint_ReturnsMethodNotAllowed()
    {
        // Arrange
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);
        string listId = Guid.NewGuid().ToString();
        string todoId = Guid.NewGuid().ToString();

        // Act - GET to individual todo URL should return method not allowed
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists/{listId}/todos/{todoId}?userId={userId}");

        // Assert
        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }
}
