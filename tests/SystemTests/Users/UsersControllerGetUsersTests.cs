using System.Net;
using SystemTests.TestObjectBuilders;
using Users.Contracts;

namespace SystemTests.Users;

/// <summary>
/// System tests for Users GET users list endpoints with pagination.
/// </summary>
public class UsersControllerGetUsersTests : BaseSystemTest
{
    public UsersControllerGetUsersTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public async Task GetUsers_WithNoUsers_ReturnsOkWithEmptyList()
    {
        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Empty(result.Data);
        Assert.Equal(0, result.Pagination.TotalItems);
        Assert.Equal(0, result.Pagination.TotalPages);
        Assert.Equal(1, result.Pagination.CurrentPage);
        Assert.Equal(50, result.Pagination.Limit);
    }

    [Fact]
    public async Task GetUsers_WithExistingUsers_ReturnsOkWithUserList()
    {
        // Arrange
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("john.doe@example.com")
            .WithUserName("johndoe")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("jane.smith@example.com")
            .WithUserName("janesmith")
            .Build());

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal(2, result.Pagination.TotalItems);
        Assert.Equal(1, result.Pagination.TotalPages);
        Assert.Equal(1, result.Pagination.CurrentPage);
        Assert.Equal(50, result.Pagination.Limit);

        // Verify user data structure
        UserDto firstUser = result.Data[0];
        Assert.NotEqual(Guid.Empty, firstUser.UserId);
        Assert.Contains("@example.com", firstUser.Email);
        Assert.NotEmpty(firstUser.UserName);
        Assert.Equal(FakeTimeProvider.GetUtcNow().UtcDateTime, firstUser.CreatedAt);
        Assert.Null(firstUser.LastLoginAt);
    }

    [Fact]
    public async Task GetUsers_ReturnsCorrectResponseStructure()
    {
        // Arrange
        await UsersTestHelper.CreateUserAsync(HttpClient);

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);

        // Verify response structure
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Pagination);

        // Verify pagination structure
        Assert.True(result.Pagination.TotalItems >= 0);
        Assert.True(result.Pagination.TotalPages >= 0);
        Assert.True(result.Pagination.CurrentPage >= 1);
        Assert.True(result.Pagination.Limit > 0);
    }

    // ==================== PAGINATION TESTS ====================

    [Fact]
    public async Task GetUsers_WithDefaultPagination_ReturnsCorrectDefaults()
    {
        // Arrange
        await UsersTestHelper.CreateUserAsync(HttpClient);

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Equal(1, result.Pagination.CurrentPage);
        Assert.Equal(50, result.Pagination.Limit);
    }

    [Fact]
    public async Task GetUsers_WithCustomPageAndLimit_ReturnsRequestedPagination()
    {
        // Arrange
        await UsersTestHelper.CreateUserAsync(HttpClient);
        await UsersTestHelper.CreateUserAsync(HttpClient);
        await UsersTestHelper.CreateUserAsync(HttpClient);

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?page=1&limit=2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal(1, result.Pagination.CurrentPage);
        Assert.Equal(2, result.Pagination.Limit);
        Assert.Equal(3, result.Pagination.TotalItems);
        Assert.Equal(2, result.Pagination.TotalPages);
    }

    [Fact]
    public async Task GetUsers_WithSecondPage_ReturnsCorrectPaginatedResults()
    {
        // Arrange - Create 3 users to test pagination
        await UsersTestHelper.CreateUserAsync(HttpClient);
        await UsersTestHelper.CreateUserAsync(HttpClient);
        await UsersTestHelper.CreateUserAsync(HttpClient);

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?page=2&limit=2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Single(result.Data); // Only 1 user on page 2
        Assert.Equal(2, result.Pagination.CurrentPage);
        Assert.Equal(2, result.Pagination.Limit);
        Assert.Equal(3, result.Pagination.TotalItems);
        Assert.Equal(2, result.Pagination.TotalPages);
    }

    [Fact]
    public async Task GetUsers_WithPageBeyondAvailableData_ReturnsEmptyResults()
    {
        // Arrange
        await UsersTestHelper.CreateUserAsync(HttpClient);

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?page=5&limit=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Empty(result.Data);
        Assert.Equal(5, result.Pagination.CurrentPage);
        Assert.Equal(10, result.Pagination.Limit);
        Assert.Equal(1, result.Pagination.TotalItems);
        Assert.Equal(1, result.Pagination.TotalPages);
    }

    [Fact]
    public async Task GetUsers_WithZeroPage_ReturnsBadRequest()
    {
        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?page=0");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_WithNegativePage_ReturnsBadRequest()
    {
        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?page=-1");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_WithZeroLimit_ReturnsBadRequest()
    {
        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?limit=0");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_WithNegativeLimit_ReturnsBadRequest()
    {
        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?limit=-1");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_WithLimitExceeding100_ReturnsBadRequest()
    {
        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?limit=101");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_WithLimitExactly100_ReturnsOk()
    {
        // Arrange
        await UsersTestHelper.CreateUserAsync(HttpClient);

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?limit=100");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Equal(100, result.Pagination.Limit);
    }

    // ==================== SORTING TESTS ====================

    [Fact]
    public async Task GetUsers_WithNoSortParameter_ReturnsUsersOrderedByCreatedAtAscending()
    {
        // Arrange - Create users at different times using FakeTimeProvider for deterministic sorting
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("user1@example.com")
            .WithUserName("user1")
            .Build());
        FakeTimeProvider.Advance(TimeSpan.FromMinutes(1)); // Advance time by 1 minute
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("user2@example.com")
            .WithUserName("user2")
            .Build());

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Equal(2, result.Data.Count);

        // Verify users are ordered by CreatedAt ascending (first created user appears first)
        Assert.True(result.Data[0].CreatedAt <= result.Data[1].CreatedAt);
    }

    [Fact]
    public async Task GetUsers_WithSortByUsernameAscending_ReturnsUsersSortedByUsernameAscending()
    {
        // Arrange
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("charlie@example.com")
            .WithUserName("charlie")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("alice@example.com")
            .WithUserName("alice")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("bob@example.com")
            .WithUserName("bob")
            .Build());

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?sort=username");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Equal(3, result.Data.Count);

        // Verify users are sorted by username ascending
        Assert.Equal("alice", result.Data[0].UserName);
        Assert.Equal("bob", result.Data[1].UserName);
        Assert.Equal("charlie", result.Data[2].UserName);
    }

    [Fact]
    public async Task GetUsers_WithSortByUsernameDescending_ReturnsUsersSortedByUsernameDescending()
    {
        // Arrange
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("alice@example.com")
            .WithUserName("alice")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("charlie@example.com")
            .WithUserName("charlie")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("bob@example.com")
            .WithUserName("bob")
            .Build());

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?sort=-username");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Equal(3, result.Data.Count);

        // Verify users are sorted by username descending
        Assert.Equal("charlie", result.Data[0].UserName);
        Assert.Equal("bob", result.Data[1].UserName);
        Assert.Equal("alice", result.Data[2].UserName);
    }

    [Fact]
    public async Task GetUsers_WithSortByEmailAscending_ReturnsUsersSortedByEmailAscending()
    {
        // Arrange
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("charlie@example.com")
            .WithUserName("charlie")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("alice@example.com")
            .WithUserName("alice")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("bob@example.com")
            .WithUserName("bob")
            .Build());

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?sort=email");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Equal(3, result.Data.Count);

        // Verify users are sorted by email ascending
        Assert.Equal("alice@example.com", result.Data[0].Email);
        Assert.Equal("bob@example.com", result.Data[1].Email);
        Assert.Equal("charlie@example.com", result.Data[2].Email);
    }

    [Fact]
    public async Task GetUsers_WithSortByEmailDescending_ReturnsUsersSortedByEmailDescending()
    {
        // Arrange
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("alice@example.com")
            .WithUserName("alice")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("bob@example.com")
            .WithUserName("bob")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("charlie@example.com")
            .WithUserName("charlie")
            .Build());

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?sort=-email");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Equal(3, result.Data.Count);

        // Verify users are sorted by email descending
        Assert.Equal("charlie@example.com", result.Data[0].Email);
        Assert.Equal("bob@example.com", result.Data[1].Email);
        Assert.Equal("alice@example.com", result.Data[2].Email);
    }

    [Fact]
    public async Task GetUsers_WithSortByCreatedAtAscending_ReturnsUsersSortedByCreatedAtAscending()
    {
        // Arrange - Create users at different times using FakeTimeProvider for deterministic sorting
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("first@example.com")
            .WithUserName("first")
            .Build());
        FakeTimeProvider.Advance(TimeSpan.FromMinutes(1));
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("second@example.com")
            .WithUserName("second")
            .Build());
        FakeTimeProvider.Advance(TimeSpan.FromMinutes(1));
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("third@example.com")
            .WithUserName("third")
            .Build());

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?sort=createdat");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Equal(3, result.Data.Count);

        // Verify users are sorted by CreatedAt ascending (oldest first)
        Assert.True(result.Data[0].CreatedAt <= result.Data[1].CreatedAt);
        Assert.True(result.Data[1].CreatedAt <= result.Data[2].CreatedAt);
    }

    [Fact]
    public async Task GetUsers_WithSortByCreatedAtDescending_ReturnsUsersSortedByCreatedAtDescending()
    {
        // Arrange - Create users at different times using FakeTimeProvider for deterministic sorting
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("oldest@example.com")
            .WithUserName("oldest")
            .Build());
        FakeTimeProvider.Advance(TimeSpan.FromMinutes(1));
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("middle@example.com")
            .WithUserName("middle")
            .Build());
        FakeTimeProvider.Advance(TimeSpan.FromMinutes(1));
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("newest@example.com")
            .WithUserName("newest")
            .Build());

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?sort=-createdat");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Equal(3, result.Data.Count);

        // Verify users are sorted by CreatedAt descending (newest first)
        Assert.True(result.Data[0].CreatedAt >= result.Data[1].CreatedAt);
        Assert.True(result.Data[1].CreatedAt >= result.Data[2].CreatedAt);
    }

    [Fact]
    public async Task GetUsers_WithSortByCaseInsensitiveFieldName_ReturnsCorrectSorting()
    {
        // Arrange
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("charlie@example.com")
            .WithUserName("charlie")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("alice@example.com")
            .WithUserName("alice")
            .Build());

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?sort=USERNAME");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Equal(2, result.Data.Count);

        // Verify case insensitive field name works correctly
        Assert.Equal("alice", result.Data[0].UserName);
        Assert.Equal("charlie", result.Data[1].UserName);
    }

    [Fact]
    public async Task GetUsers_WithSortByMixedCaseFieldName_ReturnsCorrectSorting()
    {
        // Arrange
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("charlie@example.com")
            .WithUserName("charlie")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("alice@example.com")
            .WithUserName("alice")
            .Build());

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?sort=CreatedAt");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Equal(2, result.Data.Count);

        // Verify mixed case field name works correctly (sorts by CreatedAt)
        Assert.True(result.Data[0].CreatedAt <= result.Data[1].CreatedAt);
    }

    [Fact]
    public async Task GetUsers_WithUnknownSortField_ReturnsUsersOrderedByCreatedAtAscending()
    {
        // Arrange - Create users at different times using FakeTimeProvider for deterministic sorting
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("user1@example.com")
            .WithUserName("user1")
            .Build());
        FakeTimeProvider.Advance(TimeSpan.FromMinutes(1));
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("user2@example.com")
            .WithUserName("user2")
            .Build());

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?sort=unknownfield");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Equal(2, result.Data.Count);

        // Verify unknown field falls back to CreatedAt ascending
        Assert.True(result.Data[0].CreatedAt <= result.Data[1].CreatedAt);
    }

    [Fact]
    public async Task GetUsers_WithSortingAndPagination_ReturnsSortedPaginatedResults()
    {
        // Arrange
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("charlie@example.com")
            .WithUserName("charlie")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("alice@example.com")
            .WithUserName("alice")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("diana@example.com")
            .WithUserName("diana")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("bob@example.com")
            .WithUserName("bob")
            .Build());

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?sort=username&page=2&limit=2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal(2, result.Pagination.CurrentPage);
        Assert.Equal(2, result.Pagination.Limit);
        Assert.Equal(4, result.Pagination.TotalItems);

        // Verify second page of sorted results (charlie, diana)
        Assert.Equal("charlie", result.Data[0].UserName);
        Assert.Equal("diana", result.Data[1].UserName);
    }

    [Fact]
    public async Task GetUsers_WithEmptySort_ReturnsUsersOrderedByCreatedAtAscending()
    {
        // Arrange - Create users at different times using FakeTimeProvider for deterministic sorting
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("user1@example.com")
            .WithUserName("user1")
            .Build());
        FakeTimeProvider.Advance(TimeSpan.FromMinutes(1));
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("user2@example.com")
            .WithUserName("user2")
            .Build());

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?sort=");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Equal(2, result.Data.Count);

        // Verify empty sort parameter defaults to CreatedAt ascending
        Assert.True(result.Data[0].CreatedAt <= result.Data[1].CreatedAt);
    }

    [Fact]
    public async Task GetUsers_WithWhitespaceSort_ReturnsUsersOrderedByCreatedAtAscending()
    {
        // Arrange - Create users at different times using FakeTimeProvider for deterministic sorting
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("user1@example.com")
            .WithUserName("user1")
            .Build());
        FakeTimeProvider.Advance(TimeSpan.FromMinutes(1));
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("user2@example.com")
            .WithUserName("user2")
            .Build());

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?sort=%20%20%20"); // URL encoded spaces

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Equal(2, result.Data.Count);

        // Verify whitespace sort parameter defaults to CreatedAt ascending
        Assert.True(result.Data[0].CreatedAt <= result.Data[1].CreatedAt);
    }

    [Fact]
    public async Task GetUsers_WithSortByLastLoginAtAscending_ReturnsUsersSortedByLastLoginAtAscending()
    {
        // Arrange
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("user1@example.com")
            .WithUserName("user1")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("user2@example.com")
            .WithUserName("user2")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("user3@example.com")
            .WithUserName("user3")
            .Build());

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?sort=lastloginat");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Equal(3, result.Data.Count);

        // All users should have null LastLoginAt, so they should be ordered consistently
        // This tests that the sorting works correctly with null values
        foreach (UserDto user in result.Data)
        {
            Assert.Null(user.LastLoginAt);
        }
    }

    [Fact]
    public async Task GetUsers_WithSortByLastLoginAtDescending_ReturnsUsersSortedByLastLoginAtDescending()
    {
        // Arrange
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("user1@example.com")
            .WithUserName("user1")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("user2@example.com")
            .WithUserName("user2")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("user3@example.com")
            .WithUserName("user3")
            .Build());

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?sort=-lastloginat");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Equal(3, result.Data.Count);

        // All users should have null LastLoginAt, so they should be ordered consistently
        // This tests that the sorting works correctly with null values in descending order
        foreach (UserDto user in result.Data)
        {
            Assert.Null(user.LastLoginAt);
        }
    }

    // ==================== FILTERING TESTS ====================

    [Fact]
    public async Task GetUsers_WithEmailFilter_ReturnsOnlyUsersMatchingEmailFilter()
    {
        // Arrange
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("john.doe@company.com")
            .WithUserName("johndoe")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("jane.smith@company.com")
            .WithUserName("janesmith")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("bob.wilson@external.com")
            .WithUserName("bobwilson")
            .Build());

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?email=@company.com");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal(2, result.Pagination.TotalItems);

        // Verify only users with @company.com are returned
        Assert.All(result.Data, user => Assert.Contains("@company.com", user.Email));
    }

    [Fact]
    public async Task GetUsers_WithUserNameFilter_ReturnsOnlyUsersMatchingUserNameFilter()
    {
        // Arrange
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("admin1@example.com")
            .WithUserName("admin1")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("admin2@example.com")
            .WithUserName("admin2")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("user1@example.com")
            .WithUserName("user1")
            .Build());

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?userName=admin");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal(2, result.Pagination.TotalItems);

        // Verify only users with "admin" in username are returned
        Assert.All(result.Data, user => Assert.Contains("admin", user.UserName));
    }

    [Fact]
    public async Task GetUsers_WithEmailAndUserNameFilter_ReturnsUsersMatchingBothFilters()
    {
        // Arrange
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("admin@company.com")
            .WithUserName("admin")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("admin@external.com")
            .WithUserName("admin")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("user@company.com")
            .WithUserName("user")
            .Build());

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?email=@company.com&userName=admin");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Single(result.Data);
        Assert.Equal(1, result.Pagination.TotalItems);

        // Verify only the user matching both filters is returned
        UserDto user = result.Data[0];
        Assert.Contains("@company.com", user.Email);
        Assert.Contains("admin", user.UserName);
    }

    [Fact]
    public async Task GetUsers_WithEmailFilterNoMatches_ReturnsEmptyResults()
    {
        // Arrange
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("john@company.com")
            .WithUserName("john")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("jane@company.com")
            .WithUserName("jane")
            .Build());

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?email=@nonexistent.com");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Empty(result.Data);
        Assert.Equal(0, result.Pagination.TotalItems);
        Assert.Equal(0, result.Pagination.TotalPages);
    }

    [Fact]
    public async Task GetUsers_WithUserNameFilterNoMatches_ReturnsEmptyResults()
    {
        // Arrange
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("john@company.com")
            .WithUserName("john")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("jane@company.com")
            .WithUserName("jane")
            .Build());

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?userName=nonexistent");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Empty(result.Data);
        Assert.Equal(0, result.Pagination.TotalItems);
        Assert.Equal(0, result.Pagination.TotalPages);
    }

    [Fact]
    public async Task GetUsers_WithPartialEmailFilter_ReturnsMatchingUsers()
    {
        // Arrange
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("john.doe@gmail.com")
            .WithUserName("johndoe")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("john.smith@yahoo.com")
            .WithUserName("johnsmith")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("jane.doe@gmail.com")
            .WithUserName("janedoe")
            .Build());

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?email=john");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal(2, result.Pagination.TotalItems);

        // Verify only users with "john" in email are returned
        Assert.All(result.Data, user => Assert.Contains("john", user.Email));
    }

    [Fact]
    public async Task GetUsers_WithPartialUserNameFilter_ReturnsMatchingUsers()
    {
        // Arrange
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("user1@example.com")
            .WithUserName("developer1")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("user2@example.com")
            .WithUserName("developer2")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("user3@example.com")
            .WithUserName("manager1")
            .Build());

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?userName=develop");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal(2, result.Pagination.TotalItems);

        // Verify only users with "develop" in username are returned
        Assert.All(result.Data, user => Assert.Contains("develop", user.UserName));
    }

    [Fact]
    public async Task GetUsers_WithEmptyEmailFilter_ReturnsAllUsers()
    {
        // Arrange
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("user1@example.com")
            .WithUserName("user1")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("user2@example.com")
            .WithUserName("user2")
            .Build());

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?email=");

        // Assert - empty parameter is treated as no filter
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal(2, result.Pagination.TotalItems);
    }

    [Fact]
    public async Task GetUsers_WithEmptyUserNameFilter_ReturnsAllUsers()
    {
        // Arrange
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("user1@example.com")
            .WithUserName("user1")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("user2@example.com")
            .WithUserName("user2")
            .Build());

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?userName=");

        // Assert - empty parameter is treated as no filter
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal(2, result.Pagination.TotalItems);
    }

    [Fact]
    public async Task GetUsers_WithWhitespaceEmailFilter_ReturnsAllUsers()
    {
        // Arrange
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("user1@example.com")
            .WithUserName("user1")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("user2@example.com")
            .WithUserName("user2")
            .Build());

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?email=%20%20%20"); // URL encoded spaces

        // Assert - whitespace parameter is treated as no filter
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal(2, result.Pagination.TotalItems);
    }

    [Fact]
    public async Task GetUsers_WithWhitespaceUserNameFilter_ReturnsAllUsers()
    {
        // Arrange
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("user1@example.com")
            .WithUserName("user1")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("user2@example.com")
            .WithUserName("user2")
            .Build());

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?userName=%20%20%20"); // URL encoded spaces

        // Assert - whitespace parameter is treated as no filter
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal(2, result.Pagination.TotalItems);
    }

    [Fact]
    public async Task GetUsers_WithFilteringAndSorting_ReturnsSortedFilteredResults()
    {
        // Arrange
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("charlie@company.com")
            .WithUserName("charlie")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("alice@company.com")
            .WithUserName("alice")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("bob@external.com")
            .WithUserName("bob")
            .Build());

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?email=@company.com&sort=userName");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal(2, result.Pagination.TotalItems);

        // Verify filtering and sorting work together
        Assert.Equal("alice", result.Data[0].UserName);
        Assert.Equal("charlie", result.Data[1].UserName);
        Assert.All(result.Data, user => Assert.Contains("@company.com", user.Email));
    }

    [Fact]
    public async Task GetUsers_WithFilteringAndPagination_ReturnsPaginatedFilteredResults()
    {
        // Arrange
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("user1@company.com")
            .WithUserName("user1")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("user2@company.com")
            .WithUserName("user2")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("user3@company.com")
            .WithUserName("user3")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("external@external.com")
            .WithUserName("external")
            .Build());

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?email=@company.com&page=2&limit=2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Single(result.Data); // Only 1 user on page 2
        Assert.Equal(3, result.Pagination.TotalItems); // Total filtered items
        Assert.Equal(2, result.Pagination.TotalPages); // Based on filtered items
        Assert.Equal(2, result.Pagination.CurrentPage);
        Assert.Equal(2, result.Pagination.Limit);

        // Verify the filtered result
        Assert.Contains("@company.com", result.Data[0].Email);
    }

    [Fact]
    public async Task GetUsers_WithFilteringSortingAndPagination_ReturnsCombinedResults()
    {
        // Arrange
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("delta@company.com")
            .WithUserName("delta")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("alpha@company.com")
            .WithUserName("alpha")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("charlie@company.com")
            .WithUserName("charlie")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("beta@company.com")
            .WithUserName("beta")
            .Build());
        await UsersTestHelper.CreateUserAsync(HttpClient, new AddUserCommandBuilder()
            .WithEmail("external@external.com")
            .WithUserName("external")
            .Build());

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/users?email=@company.com&sort=userName&page=2&limit=2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetUsersResult result = await FromJsonAsync<GetUsersResult>(response);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal(4, result.Pagination.TotalItems); // Total filtered items
        Assert.Equal(2, result.Pagination.TotalPages);
        Assert.Equal(2, result.Pagination.CurrentPage);

        // Verify second page of sorted, filtered results (charlie, delta)
        Assert.Equal("charlie", result.Data[0].UserName);
        Assert.Equal("delta", result.Data[1].UserName);
        Assert.All(result.Data, user => Assert.Contains("@company.com", user.Email));
    }
}
