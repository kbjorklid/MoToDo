using System.Net;
using Microsoft.Extensions.DependencyInjection;
using SystemTests.TestObjectBuilders;
using Users.Contracts;
using Users.Domain;
using Users.Infrastructure;

namespace SystemTests.Users;

/// <summary>
/// System tests for Users DELETE endpoints.
/// </summary>
public class UsersControllerDeleteTests : BaseSystemTest
{
    public UsersControllerDeleteTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public async Task DeleteUser_UserExists_ReturnsNoContent()
    {
        // Arrange
        Guid userId = await UsersTestHelper.CreateUserAsync(HttpClient);

        // Act
        HttpResponseMessage response = await HttpClient.DeleteAsync($"/api/v1/users/{userId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_UserExists_RemovesUserFromDatabase()
    {
        // Arrange
        Guid userId = await UsersTestHelper.CreateUserAsync(HttpClient);

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
        Guid userId = await UsersTestHelper.CreateUserAsync(HttpClient);

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
        Guid userId = await UsersTestHelper.CreateUserAsync(HttpClient, command);

        // Act
        HttpResponseMessage firstDeleteResponse = await HttpClient.DeleteAsync($"/api/v1/users/{userId}");
        HttpResponseMessage secondDeleteResponse = await HttpClient.DeleteAsync($"/api/v1/users/{userId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, firstDeleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, secondDeleteResponse.StatusCode);
    }
}
