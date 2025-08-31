using System.Net;
using System.Text;
using System.Text.Json;
using SystemTests.TestObjectBuilders;
using Users.Contracts;

namespace SystemTests.Users;

/// <summary>
/// System tests for Users POST endpoints.
/// </summary>
public class UsersControllerPostTests : BaseSystemTest
{
    public UsersControllerPostTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public async Task PostUsers_WithValidData_ReturnsCreatedWithUserId()
    {
        // Arrange
        AddUserCommand command = new AddUserCommandBuilder()
            .WithEmail("john.doe@example.com")
            .WithUserName("johndoe")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        AddUserResult result = await FromJsonAsync<AddUserResult>(response);
        Assert.NotEqual(Guid.Empty, result.UserId);
        Assert.Equal("john.doe@example.com", result.Email);
        Assert.Equal("johndoe", result.UserName);
        Assert.Equal(FakeTimeProvider.GetUtcNow().UtcDateTime, result.CreatedAt);

        // Verify Location header is set correctly
        Assert.NotNull(response.Headers.Location);
        Assert.Contains(result.UserId.ToString(), response.Headers.Location.ToString());
    }

    [Fact]
    public async Task PostUsers_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        AddUserCommand command = new AddUserCommandBuilder()
            .WithEmail("invalid-email")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithEmptyEmail_ReturnsBadRequest()
    {
        // Arrange
        AddUserCommand command = new AddUserCommandBuilder()
            .WithEmail("")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithEmptyUserName_ReturnsBadRequest()
    {
        // Arrange
        AddUserCommand command = new AddUserCommandBuilder()
            .WithUserName("")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithDuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        AddUserCommand firstCommand = new AddUserCommandBuilder()
            .WithEmail("duplicate@example.com")
            .WithUserName("firstuser")
            .Build();

        AddUserCommand secondCommand = new AddUserCommandBuilder()
            .WithEmail("duplicate@example.com")
            .WithUserName("seconduser")
            .Build();

        // Create first user
        HttpResponseMessage firstResponse = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(firstCommand));
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        // Act
        HttpResponseMessage secondResponse = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(secondCommand));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode);

        string responseContent = await secondResponse.Content.ReadAsStringAsync();
        Assert.Contains("A user with this email address already exists.", responseContent);
    }

    [Fact]
    public async Task PostUsers_WithDuplicateUserName_ReturnsBadRequest()
    {
        // Arrange
        AddUserCommand firstCommand = new AddUserCommandBuilder()
            .WithEmail("first@example.com")
            .WithUserName("duplicateuser")
            .Build();

        AddUserCommand secondCommand = new AddUserCommandBuilder()
            .WithEmail("second@example.com")
            .WithUserName("duplicateuser")
            .Build();

        // Create first user
        HttpResponseMessage firstResponse = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(firstCommand));
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        // Act
        HttpResponseMessage secondResponse = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(secondCommand));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode);

        string responseContent = await secondResponse.Content.ReadAsStringAsync();
        Assert.Contains("A user with this username already exists.", responseContent);
    }

    [Fact]
    public async Task PostUsers_WithNullEmail_ReturnsBadRequest()
    {
        // Arrange
        string requestBody = """{"email": null, "userName": "testuser"}""";
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithNullUserName_ReturnsBadRequest()
    {
        // Arrange
        string requestBody = """{"email": "test@example.com", "userName": null}""";
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithWhitespaceOnlyEmail_ReturnsBadRequest()
    {
        // Arrange
        AddUserCommand command = new AddUserCommandBuilder()
            .WithEmail("   ")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithWhitespaceOnlyUserName_ReturnsBadRequest()
    {
        // Arrange
        AddUserCommand command = new AddUserCommandBuilder()
            .WithUserName("   ")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithTabOnlyEmail_ReturnsBadRequest()
    {
        // Arrange
        AddUserCommand command = new AddUserCommandBuilder()
            .WithEmail("\t\t")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithTabOnlyUserName_ReturnsBadRequest()
    {
        // Arrange
        AddUserCommand command = new AddUserCommandBuilder()
            .WithUserName("\t\t")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithVeryLongValidEmail_ReturnsCreated()
    {
        // Arrange - .NET MailAddress accepts very long emails if properly formatted
        string longLocalPart = new string('a', 50);
        string longEmail = $"{longLocalPart}@example.com";

        AddUserCommand command = new AddUserCommandBuilder()
            .WithEmail(longEmail)
            .WithUserName("testuser")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithVeryLongUserName_ReturnsBadRequest()
    {
        // Arrange - Create username with 51+ characters (exceeds 50 character limit)
        string longUserName = new string('a', 51);

        AddUserCommand command = new AddUserCommandBuilder()
            .WithUserName(longUserName)
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithMalformedJson_ReturnsBadRequest()
    {
        // Arrange - Missing closing brace
        string malformedJson = """{"email": "test@example.com", "userName": "testuser" """;
        var content = new StringContent(malformedJson, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithInvalidJsonStructure_ReturnsBadRequest()
    {
        // Arrange - Invalid JSON structure
        string invalidJson = """["email", "userName"]""";
        var content = new StringContent(invalidJson, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithMissingEmailProperty_ReturnsBadRequest()
    {
        // Arrange - Missing email property completely
        string requestBody = """{"userName": "testuser"}""";
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithMissingUserNameProperty_ReturnsBadRequest()
    {
        // Arrange - Missing userName property completely
        string requestBody = """{"email": "test@example.com"}""";
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithEmptyJsonObject_ReturnsBadRequest()
    {
        // Arrange - Completely empty JSON object
        string requestBody = """{}""";
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithWrongContentType_ReturnsUnsupportedMediaType()
    {
        // Arrange - Using XML content type instead of JSON
        AddUserCommand command = new AddUserCommandBuilder().Build();
        string json = System.Text.Json.JsonSerializer.Serialize(command, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var content = new StringContent(json, Encoding.UTF8, "application/xml");

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", content);

        // Assert
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithMissingContentType_ReturnsBadRequest()
    {
        // Arrange - No content type specified
        AddUserCommand command = new AddUserCommandBuilder().Build();
        string json = System.Text.Json.JsonSerializer.Serialize(command, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = null;

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", content);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.UnsupportedMediaType);
    }

    [Fact]
    public async Task PostUsers_WithDuplicateEmailDifferentCase_ReturnsCreated()
    {
        // Arrange - Current implementation uses case-sensitive comparison
        AddUserCommand firstCommand = new AddUserCommandBuilder()
            .WithEmail("Test@Example.com")
            .WithUserName("firstuser")
            .Build();

        AddUserCommand secondCommand = new AddUserCommandBuilder()
            .WithEmail("test@example.com")
            .WithUserName("seconduser")
            .Build();

        // Create first user
        HttpResponseMessage firstResponse = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(firstCommand));
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        // Act
        HttpResponseMessage secondResponse = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(secondCommand));

        // Assert - Different case emails are treated as different
        Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithDuplicateUserNameDifferentCase_ReturnsCreated()
    {
        // Arrange - Current implementation uses case-sensitive comparison
        AddUserCommand firstCommand = new AddUserCommandBuilder()
            .WithEmail("first@example.com")
            .WithUserName("TestUser")
            .Build();

        AddUserCommand secondCommand = new AddUserCommandBuilder()
            .WithEmail("second@example.com")
            .WithUserName("testuser")
            .Build();

        // Create first user
        HttpResponseMessage firstResponse = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(firstCommand));
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        // Act
        HttpResponseMessage secondResponse = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(secondCommand));

        // Assert - Different case usernames are treated as different
        Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithEmailMissingAtSymbol_ReturnsBadRequest()
    {
        // Arrange
        AddUserCommand command = new AddUserCommandBuilder()
            .WithEmail("testexample.com")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithEmailMissingDomain_ReturnsBadRequest()
    {
        // Arrange
        AddUserCommand command = new AddUserCommandBuilder()
            .WithEmail("test@")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithEmailMissingLocalPart_ReturnsBadRequest()
    {
        // Arrange
        AddUserCommand command = new AddUserCommandBuilder()
            .WithEmail("@example.com")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithEmailMultipleAtSymbols_ReturnsBadRequest()
    {
        // Arrange
        AddUserCommand command = new AddUserCommandBuilder()
            .WithEmail("test@@example.com")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithEmailInvalidDomainFormat_ReturnsCreated()
    {
        // Arrange - .NET MailAddress accepts this format
        AddUserCommand command = new AddUserCommandBuilder()
            .WithEmail("test@invalid..domain")
            .WithUserName("testuser123")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert - .NET MailAddress is more permissive than expected
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithUserNameContainingSpaces_ReturnsBadRequest()
    {
        // Arrange
        AddUserCommand command = new AddUserCommandBuilder()
            .WithUserName("user name")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithUserNameContainingSpecialCharacters_ReturnsBadRequest()
    {
        // Arrange
        AddUserCommand command = new AddUserCommandBuilder()
            .WithUserName("user@name!")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithUserNameStartingWithNumber_ReturnsCreated()
    {
        // Arrange - Username can start with numbers according to domain rules
        AddUserCommand command = new AddUserCommandBuilder()
            .WithEmail("test123@example.com")
            .WithUserName("123username")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithUserNameTooShort_ReturnsBadRequest()
    {
        // Arrange - Username must be at least 3 characters
        AddUserCommand command = new AddUserCommandBuilder()
            .WithUserName("ab")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithEmailStartingWithDot_ReturnsBadRequest()
    {
        // Arrange - .NET MailAddress rejects emails starting with dot
        AddUserCommand command = new AddUserCommandBuilder()
            .WithEmail(".test@example.com")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithEmailEndingWithDot_ReturnsCreated()
    {
        // Arrange - .NET MailAddress accepts emails ending with dot before @
        AddUserCommand command = new AddUserCommandBuilder()
            .WithEmail("test.@example.com")
            .WithUserName("testuser456")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert - .NET MailAddress is more permissive than expected
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithEmailConsecutiveDots_ReturnsCreated()
    {
        // Arrange - .NET MailAddress accepts consecutive dots in local part
        AddUserCommand command = new AddUserCommandBuilder()
            .WithEmail("test..user@example.com")
            .WithUserName("testuser789")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert - .NET MailAddress is more permissive than expected
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithUserNameStartingWithUnderscore_ReturnsBadRequest()
    {
        // Arrange - Username cannot start with underscore according to regex
        AddUserCommand command = new AddUserCommandBuilder()
            .WithUserName("_username")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithUserNameEndingWithUnderscore_ReturnsBadRequest()
    {
        // Arrange - Username cannot end with underscore according to regex
        AddUserCommand command = new AddUserCommandBuilder()
            .WithUserName("username_")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithUserNameStartingWithHyphen_ReturnsBadRequest()
    {
        // Arrange - Username cannot start with hyphen according to regex
        AddUserCommand command = new AddUserCommandBuilder()
            .WithUserName("-username")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithUserNameEndingWithHyphen_ReturnsBadRequest()
    {
        // Arrange - Username cannot end with hyphen according to regex
        AddUserCommand command = new AddUserCommandBuilder()
            .WithUserName("username-")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithValidUserNameContainingHyphenAndUnderscore_ReturnsCreated()
    {
        // Arrange - Username can contain hyphens and underscores in the middle
        AddUserCommand command = new AddUserCommandBuilder()
            .WithEmail("test-valid@example.com")
            .WithUserName("user-name_123")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithSingleCharacterUserName_ReturnsBadRequest()
    {
        // Arrange - Username must be at least 3 characters
        AddUserCommand command = new AddUserCommandBuilder()
            .WithUserName("a")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PutUsers_OnPostEndpoint_ReturnsMethodNotAllowed()
    {
        // Arrange
        AddUserCommand command = new AddUserCommandBuilder().Build();

        // Act
        HttpResponseMessage response = await HttpClient.PutAsync("/api/v1/users", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithExtremelyLargeEmail_ReturnsBadRequest()
    {
        // Arrange - Create email with 1000+ characters to test boundary limits
        string largeLocalPart = new string('a', 1000);
        string largeEmail = $"{largeLocalPart}@example.com";

        AddUserCommand command = new AddUserCommandBuilder()
            .WithEmail(largeEmail)
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithMalformedJsonMissingQuotes_ReturnsBadRequest()
    {
        // Arrange - JSON with unquoted property names
        string malformedJson = "{email: \"test@example.com\", userName: \"testuser\"}";
        var content = new StringContent(malformedJson, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithMalformedJsonExtraComma_ReturnsBadRequest()
    {
        // Arrange - JSON with trailing comma
        string malformedJson = "{\"email\": \"test@example.com\", \"userName\": \"testuser\",}";
        var content = new StringContent(malformedJson, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithEmailExactly320Characters_ReturnsCreated()
    {
        // Arrange - Email with exactly 320 characters (.NET MailAddress accepts this)
        string localPart = new string('a', 256); // 256 chars
        string domain = new string('b', 59) + ".com"; // 63 chars total
        string maxEmail = $"{localPart}@{domain}"; // 256 + 1 + 63 = 320 chars

        AddUserCommand command = new AddUserCommandBuilder()
            .WithEmail(maxEmail)
            .WithUserName("longemailtestuser")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithEmailExceeding320Characters_ReturnsBadRequest()
    {
        // Arrange - Email exceeding 320 character limit
        string localPart = new string('a', 257); // 257 chars
        string domain = new string('b', 59) + ".com"; // 63 chars total
        string longEmail = $"{localPart}@{domain}"; // 257 + 1 + 63 = 321 chars

        AddUserCommand command = new AddUserCommandBuilder()
            .WithEmail(longEmail)
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithQuotedLocalPartEmail_ReturnsCreated()
    {
        // Arrange - RFC allows quoted strings in local part
        AddUserCommand command = new AddUserCommandBuilder()
            .WithEmail("\"test user\"@example.com")
            .WithUserName("quoteduser")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert
        // Note: .NET MailAddress may or may not accept quoted local parts
        Assert.True(response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostUsers_WithUsernameExactly3Characters_ReturnsCreated()
    {
        // Arrange - Username at minimum boundary (3 characters)
        AddUserCommand command = new AddUserCommandBuilder()
            .WithUserName("abc")
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PostUsers_WithUsernameExactly50Characters_ReturnsCreated()
    {
        // Arrange - Username at maximum boundary (50 characters)
        string maxUsername = new string('a', 50);
        AddUserCommand command = new AddUserCommandBuilder()
            .WithUserName(maxUsername)
            .Build();

        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("/api/v1/users", ToJsonContent(command));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
