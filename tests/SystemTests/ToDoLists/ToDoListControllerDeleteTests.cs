using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SystemTests.Users;
using ToDoLists.Contracts;
using ToDoLists.Infrastructure;

namespace SystemTests.ToDoLists;

/// <summary>
/// System tests for ToDoLists DELETE endpoints.
/// </summary>
public class ToDoListControllerDeleteTests : BaseSystemTest
{
    public ToDoListControllerDeleteTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public async Task DeleteToDoList_WithValidIdAndUserId_ReturnsNoContent()
    {
        // Arrange
        CreateToDoListResult createdList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, "Shopping List");
        string userId = createdList.UserId.ToString();
        string listId = createdList.ToDoListId.ToString();

        // Act
        HttpResponseMessage response = await HttpClient.DeleteAsync($"/api/v1/todo-lists/{listId}?userId={userId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteToDoList_WithValidIdAndUserId_RemovesToDoListFromDatabase()
    {
        // Arrange
        CreateToDoListResult createdList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, "Shopping List");
        string userId = createdList.UserId.ToString();
        string listId = createdList.ToDoListId.ToString();

        // Act
        HttpResponseMessage response = await HttpClient.DeleteAsync($"/api/v1/todo-lists/{listId}?userId={userId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify todo list is actually removed from database
        using IServiceScope scope = WebAppFactory.Services.CreateScope();
        ToDoListsDbContext dbContext = scope.ServiceProvider.GetRequiredService<ToDoListsDbContext>();
        var todoListId = new global::ToDoLists.Domain.ToDoListId(createdList.ToDoListId);
        bool todoListExists = await dbContext.ToDoLists.AnyAsync(tl => tl.Id == todoListId);
        Assert.False(todoListExists);
    }

    [Fact]
    public async Task DeleteToDoList_WithToDoListContainingTodos_DeletesListAndAllTodos()
    {
        // Arrange
        CreateToDoListResult createdList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, "Shopping List");
        string userId = createdList.UserId.ToString();
        string listId = createdList.ToDoListId.ToString();

        // Add multiple todos to the list
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, createdList.ToDoListId, "Buy milk");
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, createdList.ToDoListId, "Buy bread");
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, createdList.ToDoListId, "Buy eggs");

        // Act
        HttpResponseMessage response = await HttpClient.DeleteAsync($"/api/v1/todo-lists/{listId}?userId={userId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify todo list and all its todos are removed from database
        using IServiceScope scope = WebAppFactory.Services.CreateScope();
        ToDoListsDbContext dbContext = scope.ServiceProvider.GetRequiredService<ToDoListsDbContext>();
        var todoListId = new global::ToDoLists.Domain.ToDoListId(createdList.ToDoListId);
        bool todoListExists = await dbContext.ToDoLists.AnyAsync(tl => tl.Id == todoListId);
        Assert.False(todoListExists);

        // Verify cascading deletion removed all todos (EF handles this automatically)
        int todoCount = await dbContext.ToDoLists
            .Where(tl => tl.Id == todoListId)
            .SelectMany(tl => tl.Todos)
            .CountAsync();
        Assert.Equal(0, todoCount);
    }

    [Fact]
    public async Task DeleteToDoList_WithNonExistentToDoListId_ReturnsNotFound()
    {
        // Arrange
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);
        var nonExistentListId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await HttpClient.DeleteAsync($"/api/v1/todo-lists/{nonExistentListId}?userId={userId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteToDoList_WithDifferentUserId_ReturnsForbidden()
    {
        // Arrange
        CreateToDoListResult createdList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, "Shopping List");
        Guid differentUserId = await UserTestHelper.CreateUserAsync(HttpClient);
        string listId = createdList.ToDoListId.ToString();

        // Act
        HttpResponseMessage response = await HttpClient.DeleteAsync($"/api/v1/todo-lists/{listId}?userId={differentUserId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        // Verify todo list still exists in database
        using IServiceScope scope = WebAppFactory.Services.CreateScope();
        ToDoListsDbContext dbContext = scope.ServiceProvider.GetRequiredService<ToDoListsDbContext>();
        var todoListId = new global::ToDoLists.Domain.ToDoListId(createdList.ToDoListId);
        bool todoListExists = await dbContext.ToDoLists.AnyAsync(tl => tl.Id == todoListId);
        Assert.True(todoListExists);
    }

    [Theory]
    [InlineData("invalid-guid")]
    [InlineData("not-a-guid-at-all")]
    [InlineData("12345678-1234-1234-1234")]  // Incomplete GUID
    [InlineData("12345678-1234-1234-1234-12345678901234")]  // Too long
    public async Task DeleteToDoList_WithInvalidToDoListId_ReturnsBadRequest(string invalidListId)
    {
        // Arrange
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);

        // Act
        HttpResponseMessage response = await HttpClient.DeleteAsync($"/api/v1/todo-lists/{invalidListId}?userId={userId}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("invalid-guid")]
    [InlineData("not-a-guid-at-all")]
    [InlineData("12345678-1234-1234-1234")]  // Incomplete GUID
    [InlineData("12345678-1234-1234-1234-12345678901234")]  // Too long
    public async Task DeleteToDoList_WithInvalidUserId_ReturnsBadRequest(string invalidUserId)
    {
        // Arrange
        CreateToDoListResult createdList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, "Shopping List");
        string listId = createdList.ToDoListId.ToString();

        // Act
        HttpResponseMessage response = await HttpClient.DeleteAsync($"/api/v1/todo-lists/{listId}?userId={invalidUserId}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Verify todo list still exists in database
        using IServiceScope scope = WebAppFactory.Services.CreateScope();
        ToDoListsDbContext dbContext = scope.ServiceProvider.GetRequiredService<ToDoListsDbContext>();
        var todoListId = new global::ToDoLists.Domain.ToDoListId(createdList.ToDoListId);
        bool todoListExists = await dbContext.ToDoLists.AnyAsync(tl => tl.Id == todoListId);
        Assert.True(todoListExists);
    }

    [Fact]
    public async Task DeleteToDoList_WithMissingUserId_ReturnsBadRequest()
    {
        // Arrange
        CreateToDoListResult createdList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, "Shopping List");
        string listId = createdList.ToDoListId.ToString();

        // Act
        HttpResponseMessage response = await HttpClient.DeleteAsync($"/api/v1/todo-lists/{listId}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Verify todo list still exists in database
        using IServiceScope scope = WebAppFactory.Services.CreateScope();
        ToDoListsDbContext dbContext = scope.ServiceProvider.GetRequiredService<ToDoListsDbContext>();
        var todoListId = new global::ToDoLists.Domain.ToDoListId(createdList.ToDoListId);
        bool todoListExists = await dbContext.ToDoLists.AnyAsync(tl => tl.Id == todoListId);
        Assert.True(todoListExists);
    }

    [Fact]
    public async Task DeleteToDoList_AfterDeletion_GetToDoListReturnsNotFound()
    {
        // Arrange
        CreateToDoListResult createdList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, "Shopping List");
        string userId = createdList.UserId.ToString();
        string listId = createdList.ToDoListId.ToString();

        // Act - Delete the todo list
        HttpResponseMessage deleteResponse = await HttpClient.DeleteAsync($"/api/v1/todo-lists/{listId}?userId={userId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Act - Try to get the deleted todo list
        HttpResponseMessage getResponse = await HttpClient.GetAsync($"/api/v1/todo-lists/{listId}?userId={userId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteToDoList_WithValidData_DoesNotAffectOtherUsersToDoLists()
    {
        // Arrange
        CreateToDoListResult userOneList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, "User One List");
        CreateToDoListResult userTwoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, "User Two List");

        string userOneId = userOneList.UserId.ToString();
        string userOneListId = userOneList.ToDoListId.ToString();

        // Act - Delete user one's list
        HttpResponseMessage response = await HttpClient.DeleteAsync($"/api/v1/todo-lists/{userOneListId}?userId={userOneId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify user two's list still exists
        using IServiceScope scope = WebAppFactory.Services.CreateScope();
        ToDoListsDbContext dbContext = scope.ServiceProvider.GetRequiredService<ToDoListsDbContext>();
        var userTwoListId = new global::ToDoLists.Domain.ToDoListId(userTwoList.ToDoListId);
        bool userTwoListExists = await dbContext.ToDoLists.AnyAsync(tl => tl.Id == userTwoListId);
        Assert.True(userTwoListExists);
    }
}
