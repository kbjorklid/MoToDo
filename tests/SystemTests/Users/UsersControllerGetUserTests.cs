using System.Net;
using SystemTests.TestObjectBuilders;
using Users.Contracts;

namespace SystemTests.Users;

/// <summary>
/// System tests for Users GET individual user endpoints.
/// </summary>
public class UsersControllerGetUserTests : BaseSystemTest
{
    public UsersControllerGetUserTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public async Task GetUser_WhenUserExists_ReturnsOkWithUserData()
    {
        // Arrange
        AddUserCommand addCommand = new AddUserCommandBuilder()
            .WithEmail("john.doe@example.com")
            .WithUserName("johndoe")
            .Build();
        Guid userId = await UsersTestHelper.CreateUserAsync(HttpClient, addCommand);

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/users/{userId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        UserDto userDto = await FromJsonAsync<UserDto>(response);
        Assert.Equal(userId, userDto.UserId);
        Assert.Equal("john.doe@example.com", userDto.Email);
        Assert.Equal("johndoe", userDto.UserName);
        Assert.True(userDto.CreatedAt <= DateTime.UtcNow);
        Assert.True(userDto.CreatedAt >= DateTime.UtcNow.AddMinutes(-1));
        Assert.Null(userDto.LastLoginAt);
    }

    [Fact]
    public async Task GetUser_WhenUserNotFound_ReturnsNotFound()
    {
        // Act
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/users/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUser_WithInvalidUuidFormat_ReturnsBadRequest()
    {
        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users/not-a-valid-uuid");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_WithTrailingSlash_ReturnsOk()
    {
        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users/");

        // Assert
        // With the new GET /users endpoint, /api/v1/users/ now returns the users list
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_OnGetUserEndpoint_ReturnsMethodNotAllowed()
    {
        // Arrange
        AddUserCommand command = new AddUserCommandBuilder().Build();
        var userId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync($"/api/v1/users/{userId}", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task PutUsers_OnGetUserEndpoint_ReturnsMethodNotAllowed()
    {
        // Arrange
        AddUserCommand command = new AddUserCommandBuilder().Build();
        var userId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await HttpClient.PutAsync($"/api/v1/users/{userId}", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }
}
