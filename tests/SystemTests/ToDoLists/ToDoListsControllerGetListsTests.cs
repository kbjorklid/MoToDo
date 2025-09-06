using System.Net;
using SystemTests.Users;
using ToDoLists.Contracts;

namespace SystemTests.ToDoLists;

/// <summary>
/// System tests for the GET /todo-lists endpoint (list all todo lists for a user).
/// </summary>
public class ToDoListsControllerGetListsTests : BaseSystemTest
{
    public ToDoListsControllerGetListsTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }


    [Fact]
    public async Task GetToDoLists_WithValidUserId_ReturnsOkWithToDoListsSummaries()
    {
        // Arrange
        Guid userId = await UsersTestHelper.CreateUserAsync(HttpClient);
        _ = await ToDoListsTestHelper.CreateToDoListAsync(HttpClient, userId, "Shopping List");
        CreateToDoListResult list2 = await ToDoListsTestHelper.CreateToDoListAsync(HttpClient, userId, "Work Tasks");

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists?userId={userId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetToDoListsResult result = await FromJsonAsync<GetToDoListsResult>(response);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal(2, result.Pagination.TotalItems);
        Assert.Equal(1, result.Pagination.TotalPages);
        Assert.Equal(1, result.Pagination.CurrentPage);
        Assert.Equal(50, result.Pagination.Limit);

        // Verify todo lists are ordered by CreatedAt descending (newest first)
        ToDoListSummaryDto firstList = result.Data[0];
        ToDoListSummaryDto secondList = result.Data[1];
        Assert.True(firstList.CreatedAt >= secondList.CreatedAt);

        // Verify all properties are correctly populated
        Assert.All(result.Data, todoList =>
        {
            Assert.NotEqual(Guid.Empty, todoList.Id);
            Assert.NotEmpty(todoList.Title);
            Assert.Equal(0, todoList.TodoCount);
            Assert.Equal(FakeTimeProvider.GetUtcNow().UtcDateTime, todoList.CreatedAt);
            Assert.Null(todoList.UpdatedAt);
        });
    }

    [Fact]
    public async Task GetToDoLists_WithEmptyUserToDoLists_ReturnsOkWithEmptyList()
    {
        // Arrange
        Guid userId = await UsersTestHelper.CreateUserAsync(HttpClient);

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists?userId={userId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetToDoListsResult result = await FromJsonAsync<GetToDoListsResult>(response);
        Assert.Empty(result.Data);
        Assert.Equal(0, result.Pagination.TotalItems);
        Assert.Equal(0, result.Pagination.TotalPages);
        Assert.Equal(1, result.Pagination.CurrentPage);
        Assert.Equal(50, result.Pagination.Limit);
    }

    [Fact]
    public async Task GetToDoLists_WithPaginationPage2Limit3_ReturnsPaginatedResults()
    {
        // Arrange
        Guid userId = await UsersTestHelper.CreateUserAsync(HttpClient);
        for (int i = 1; i <= 5; i++)
        {
            await ToDoListsTestHelper.CreateToDoListAsync(HttpClient, userId, $"List {i}");
        }

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists?userId={userId}&page=2&limit=3");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetToDoListsResult result = await FromJsonAsync<GetToDoListsResult>(response);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal(5, result.Pagination.TotalItems);
        Assert.Equal(2, result.Pagination.TotalPages);
        Assert.Equal(2, result.Pagination.CurrentPage);
        Assert.Equal(3, result.Pagination.Limit);
    }

    [Fact]
    public async Task GetToDoLists_WithSortByTitleAscending_ReturnsSortedResults()
    {
        // Arrange
        Guid userId = await UsersTestHelper.CreateUserAsync(HttpClient);
        await ToDoListsTestHelper.CreateToDoListAsync(HttpClient, userId, "Zebra Tasks");
        await ToDoListsTestHelper.CreateToDoListAsync(HttpClient, userId, "Alpha Tasks");
        await ToDoListsTestHelper.CreateToDoListAsync(HttpClient, userId, "Beta Tasks");

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists?userId={userId}&sort=title");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetToDoListsResult result = await FromJsonAsync<GetToDoListsResult>(response);
        Assert.Equal(3, result.Data.Count);

        // Verify ascending order by title
        Assert.Equal("Alpha Tasks", result.Data[0].Title);
        Assert.Equal("Beta Tasks", result.Data[1].Title);
        Assert.Equal("Zebra Tasks", result.Data[2].Title);
    }

    [Fact]
    public async Task GetToDoLists_WithSortByTitleDescending_ReturnsSortedResults()
    {
        // Arrange
        Guid userId = await UsersTestHelper.CreateUserAsync(HttpClient);
        await ToDoListsTestHelper.CreateToDoListAsync(HttpClient, userId, "Alpha Tasks");
        await ToDoListsTestHelper.CreateToDoListAsync(HttpClient, userId, "Zebra Tasks");
        await ToDoListsTestHelper.CreateToDoListAsync(HttpClient, userId, "Beta Tasks");

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists?userId={userId}&sort=-title");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetToDoListsResult result = await FromJsonAsync<GetToDoListsResult>(response);
        Assert.Equal(3, result.Data.Count);

        // Verify descending order by title
        Assert.Equal("Zebra Tasks", result.Data[0].Title);
        Assert.Equal("Beta Tasks", result.Data[1].Title);
        Assert.Equal("Alpha Tasks", result.Data[2].Title);
    }

    [Fact]
    public async Task GetToDoLists_WithSortByCreatedAtAscending_ReturnsSortedResults()
    {
        // Arrange
        Guid userId = await UsersTestHelper.CreateUserAsync(HttpClient);

        // Create lists with time advancement to ensure different timestamps
        _ = await ToDoListsTestHelper.CreateToDoListAsync(HttpClient, userId, "First List");
        FakeTimeProvider.Advance(TimeSpan.FromMinutes(10));

        _ = await ToDoListsTestHelper.CreateToDoListAsync(HttpClient, userId, "Second List");
        FakeTimeProvider.Advance(TimeSpan.FromMinutes(10));

        _ = await ToDoListsTestHelper.CreateToDoListAsync(HttpClient, userId, "Third List");

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists?userId={userId}&sort=createdat");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetToDoListsResult result = await FromJsonAsync<GetToDoListsResult>(response);
        Assert.Equal(3, result.Data.Count);

        // Verify ascending order by creation date (oldest first)
        Assert.Equal("First List", result.Data[0].Title);
        Assert.Equal("Second List", result.Data[1].Title);
        Assert.Equal("Third List", result.Data[2].Title);
    }

    [Fact]
    public async Task GetToDoLists_WithSortByCreatedAtDescending_ReturnsSortedResults()
    {
        // Arrange
        Guid userId = await UsersTestHelper.CreateUserAsync(HttpClient);

        // Create lists with time advancement to ensure different timestamps
        _ = await ToDoListsTestHelper.CreateToDoListAsync(HttpClient, userId, "First List");
        FakeTimeProvider.Advance(TimeSpan.FromMinutes(10));

        _ = await ToDoListsTestHelper.CreateToDoListAsync(HttpClient, userId, "Second List");
        FakeTimeProvider.Advance(TimeSpan.FromMinutes(10));

        _ = await ToDoListsTestHelper.CreateToDoListAsync(HttpClient, userId, "Third List");

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists?userId={userId}&sort=-createdat");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetToDoListsResult result = await FromJsonAsync<GetToDoListsResult>(response);
        Assert.Equal(3, result.Data.Count);

        // Verify descending order by creation date (newest first) - this is also the default
        Assert.Equal("Third List", result.Data[0].Title);
        Assert.Equal("Second List", result.Data[1].Title);
        Assert.Equal("First List", result.Data[2].Title);
    }

    [Fact]
    public async Task GetToDoLists_WithInvalidUserIdFormat_ReturnsBadRequest()
    {
        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/todo-lists?userId=invalid-guid-format");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetToDoLists_WithEmptyUserId_ReturnsBadRequest()
    {
        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/todo-lists?userId=");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetToDoLists_WithEmptyGuidUserId_ReturnsBadRequest()
    {
        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/todo-lists?userId=00000000-0000-0000-0000-000000000000");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetToDoLists_WithMissingUserIdQueryParameter_ReturnsBadRequest()
    {
        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/api/v1/todo-lists");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetToDoLists_WithNonExistentUser_ReturnsOkWithEmptyList()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists?userId={nonExistentUserId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetToDoListsResult result = await FromJsonAsync<GetToDoListsResult>(response);
        Assert.Empty(result.Data);
        Assert.Equal(0, result.Pagination.TotalItems);
    }

    [Fact]
    public async Task GetToDoLists_WithInvalidPageNumber_ReturnsBadRequest()
    {
        // Arrange
        Guid userId = await UsersTestHelper.CreateUserAsync(HttpClient);

        // Act - Page 0 is invalid (pages start from 1)
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists?userId={userId}&page=0");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetToDoLists_WithInvalidLimitNumber_ReturnsBadRequest()
    {
        // Arrange
        Guid userId = await UsersTestHelper.CreateUserAsync(HttpClient);

        // Act - Limit 0 is invalid
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists?userId={userId}&limit=0");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetToDoLists_WithInvalidSortParameter_ReturnsBadRequest()
    {
        // Arrange
        Guid userId = await UsersTestHelper.CreateUserAsync(HttpClient);

        // Act - Using invalid sort parameter
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists?userId={userId}&sort=invalid-field");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetToDoLists_WithPageBeyondAvailableData_ReturnsOkWithEmptyList()
    {
        // Arrange
        Guid userId = await UsersTestHelper.CreateUserAsync(HttpClient);
        await ToDoListsTestHelper.CreateToDoListAsync(HttpClient, userId, "Only List");

        // Act - Request page 5 when only 1 item exists
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists?userId={userId}&page=5");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetToDoListsResult result = await FromJsonAsync<GetToDoListsResult>(response);
        Assert.Empty(result.Data);
        Assert.Equal(1, result.Pagination.TotalItems);
        Assert.Equal(1, result.Pagination.TotalPages);
        Assert.Equal(5, result.Pagination.CurrentPage);
    }

    [Fact]
    public async Task GetToDoLists_WithMultipleUsersData_ReturnsOnlyCurrentUserData()
    {
        // Arrange
        Guid user1 = await UsersTestHelper.CreateUserAsync(HttpClient);
        Guid user2 = await UsersTestHelper.CreateUserAsync(HttpClient);

        // Create todo lists for both users
        await ToDoListsTestHelper.CreateToDoListAsync(HttpClient, user1, "User1 List 1");
        await ToDoListsTestHelper.CreateToDoListAsync(HttpClient, user1, "User1 List 2");
        await ToDoListsTestHelper.CreateToDoListAsync(HttpClient, user2, "User2 List 1");

        // Act - Request lists for user1 only
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists?userId={user1}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetToDoListsResult result = await FromJsonAsync<GetToDoListsResult>(response);
        Assert.Equal(2, result.Data.Count);
        Assert.All(result.Data, list => Assert.Contains("User1", list.Title));
    }

    [Fact]
    public async Task GetToDoLists_WithDefaultPagination_UsesCorrectDefaults()
    {
        // Arrange
        Guid userId = await UsersTestHelper.CreateUserAsync(HttpClient);
        await ToDoListsTestHelper.CreateToDoListAsync(HttpClient, userId, "Test List");

        // Act - No pagination parameters provided
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists?userId={userId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GetToDoListsResult result = await FromJsonAsync<GetToDoListsResult>(response);
        Assert.Equal(1, result.Pagination.CurrentPage);
        Assert.Equal(50, result.Pagination.Limit); // Default limit
    }
}
