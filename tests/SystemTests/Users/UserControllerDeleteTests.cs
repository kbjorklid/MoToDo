using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SystemTests.TestObjectBuilders;
using SystemTests.ToDoLists;
using ToDoLists.Contracts;
using ToDoLists.Infrastructure;
using Users.Contracts;
using Users.Domain;
using Users.Infrastructure;

namespace SystemTests.Users;

/// <summary>
/// System tests for Users DELETE endpoints.
/// </summary>
public class UserControllerDeleteTests : BaseSystemTest
{
    public UserControllerDeleteTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public async Task DeleteUser_UserExists_ReturnsNoContent()
    {
        // Arrange
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);

        // Act
        HttpResponseMessage response = await HttpClient.DeleteAsync($"/api/v1/users/{userId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_UserExists_RemovesUserFromDatabase()
    {
        // Arrange
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);

        // Act
        HttpResponseMessage response = await HttpClient.DeleteAsync($"/api/v1/users/{userId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify user is actually removed from database
        using IServiceScope scope = WebAppFactory.Services.CreateScope();
        UsersDbContext dbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        User? userInDb = await dbContext.Users.FindAsync(new UserId(userId));
        Assert.Null(userInDb);
    }

    [Fact]
    public async Task DeleteUser_UserExists_VerifyUserCannotBeRetrievedAfterDeletion()
    {
        // Arrange
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);

        // Act
        HttpResponseMessage deleteResponse = await HttpClient.DeleteAsync($"/api/v1/users/{userId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify subsequent GET request returns NotFound
        HttpResponseMessage getResponse = await HttpClient.GetAsync($"/api/v1/users/{userId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_UserNotFound_ReturnsNotFound()
    {
        // Act
        HttpResponseMessage response = await HttpClient.DeleteAsync($"/api/v1/users/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_WithInvalidUuidFormat_ReturnsBadRequest()
    {
        // Act
        HttpResponseMessage response = await HttpClient.DeleteAsync("/api/v1/users/not-a-valid-uuid");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_WithEmptyUserId_ReturnsMethodNotAllowed()
    {
        // Act
        HttpResponseMessage response = await HttpClient.DeleteAsync("/api/v1/users/");

        // Assert
        // When the route doesn't match (empty userId), ASP.NET returns MethodNotAllowed
        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_SameUserTwice_SecondDeleteReturnsNotFound()
    {
        // Arrange
        AddUserCommand command = new AddUserCommandBuilder().Build();
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient, command);

        // Act
        HttpResponseMessage firstDeleteResponse = await HttpClient.DeleteAsync($"/api/v1/users/{userId}");
        HttpResponseMessage secondDeleteResponse = await HttpClient.DeleteAsync($"/api/v1/users/{userId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, firstDeleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, secondDeleteResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_UserWithToDoLists_DeletesAllToDoLists()
    {
        // Arrange
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);
        CreateToDoListResult firstList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, userId, "Shopping List");
        CreateToDoListResult secondList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, userId, "Work Tasks");

        // Act
        HttpResponseMessage deleteResponse = await HttpClient.DeleteAsync($"/api/v1/users/{userId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Allow time for integration events to be processed
        await Task.Delay(1000);

        // Verify user is deleted
        using IServiceScope scope = WebAppFactory.Services.CreateScope();
        UsersDbContext usersDbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        User? userInDb = await usersDbContext.Users.FindAsync(new global::Users.Domain.UserId(userId));
        Assert.Null(userInDb);

        // Verify all user's todo lists are deleted
        ToDoListsDbContext todoListsDbContext = scope.ServiceProvider.GetRequiredService<ToDoListsDbContext>();
        List<global::ToDoLists.Domain.ToDoList> todoListsInDb = await todoListsDbContext.ToDoLists
            .Where(tl => tl.UserId == new global::ToDoLists.Domain.UserId(userId))
            .ToListAsync();
        Assert.Empty(todoListsInDb);

        // Verify individual todo lists cannot be retrieved
        HttpResponseMessage firstListResponse = await HttpClient.GetAsync($"/api/v1/todo-lists/{firstList.ToDoListId}?userId={userId}");
        Assert.Equal(HttpStatusCode.NotFound, firstListResponse.StatusCode);

        HttpResponseMessage secondListResponse = await HttpClient.GetAsync($"/api/v1/todo-lists/{secondList.ToDoListId}?userId={userId}");
        Assert.Equal(HttpStatusCode.NotFound, secondListResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_UserWithNoToDoLists_DeletesUserSuccessfully()
    {
        // Arrange
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);

        // Act
        HttpResponseMessage deleteResponse = await HttpClient.DeleteAsync($"/api/v1/users/{userId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify user is deleted
        using IServiceScope scope = WebAppFactory.Services.CreateScope();
        UsersDbContext usersDbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        User? userInDb = await usersDbContext.Users.FindAsync(new global::Users.Domain.UserId(userId));
        Assert.Null(userInDb);
    }

    [Fact]
    public async Task DeleteUser_UserWithManyToDoLists_DeletesAllToDoLists()
    {
        // Arrange
        Guid userId = await UserTestHelper.CreateUserAsync(HttpClient);
        var createdLists = new List<CreateToDoListResult>();

        // Create 5 todo lists for the user
        for (int i = 1; i <= 5; i++)
        {
            CreateToDoListResult list = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, userId, $"List {i}");
            createdLists.Add(list);
        }

        // Act
        HttpResponseMessage deleteResponse = await HttpClient.DeleteAsync($"/api/v1/users/{userId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Allow time for integration events to be processed
        await Task.Delay(1000);

        // Verify user is deleted
        using IServiceScope scope = WebAppFactory.Services.CreateScope();
        UsersDbContext usersDbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        User? userInDb = await usersDbContext.Users.FindAsync(new global::Users.Domain.UserId(userId));
        Assert.Null(userInDb);

        // Verify all todo lists are deleted
        ToDoListsDbContext todoListsDbContext = scope.ServiceProvider.GetRequiredService<ToDoListsDbContext>();
        List<global::ToDoLists.Domain.ToDoList> todoListsInDb = await todoListsDbContext.ToDoLists
            .Where(tl => tl.UserId == new global::ToDoLists.Domain.UserId(userId))
            .ToListAsync();
        Assert.Empty(todoListsInDb);

        // Verify none of the todo lists can be retrieved individually
        foreach (CreateToDoListResult createdList in createdLists)
        {
            HttpResponseMessage listResponse = await HttpClient.GetAsync($"/api/v1/todo-lists/{createdList.ToDoListId}?userId={userId}");
            Assert.Equal(HttpStatusCode.NotFound, listResponse.StatusCode);
        }
    }
}
