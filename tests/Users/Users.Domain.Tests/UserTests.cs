using Base.Domain;
using Base.Domain.Result;
using Users.Domain.Tests.TestObjectBuilders;

namespace Users.Domain.Tests;

/// <summary>
/// Unit tests for the User aggregate root.
/// </summary>
public class UserTests
{
    #region Register Method Tests

    [Fact]
    public void Register_WithValidEmailAndUserName_ReturnsSuccessWithUser()
    {
        // Arrange
        DateTime createdAt = DateTime.UtcNow;

        // Act
        Result<User> result = User.Register("valid@example.com", "validuser", createdAt);

        // Assert
        Assert.True(result.IsSuccess);
        User user = result.Value;
        Assert.Equal("valid@example.com", user.Email.Value.Address);
        Assert.Equal("validuser", user.UserName.Value);
        Assert.Equal(createdAt, user.CreatedAt);
        Assert.Null(user.LastLoginAt);
        Assert.NotEqual(Guid.Empty, user.Id.Value);
    }

    [Fact]
    public void Register_WithValidData_RaisesUserRegisteredEvent()
    {
        // Arrange
        DateTime createdAt = DateTime.UtcNow;

        // Act
        Result<User> result = User.Register("valid@example.com", "validuser", createdAt);

        // Assert
        Assert.True(result.IsSuccess);
        User user = result.Value;
        IReadOnlyCollection<IDomainEvent> domainEvents = user.GetDomainEvents();
        Assert.Single(domainEvents);

        UserRegisteredEvent userRegisteredEvent = Assert.IsType<UserRegisteredEvent>(domainEvents.First());
        Assert.Equal(user.Id, userRegisteredEvent.UserId);
        Assert.Equal("valid@example.com", userRegisteredEvent.Email.Value.Address);
        Assert.Equal("validuser", userRegisteredEvent.UserName.Value);
        Assert.Equal(createdAt, userRegisteredEvent.OccurredOn);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Register_WithInvalidEmail_ReturnsFailureWithEmailEmptyError(string? invalidEmail)
    {
        // Arrange
        DateTime createdAt = DateTime.UtcNow;

        // Act
        Result<User> result = User.Register(invalidEmail!, "validuser", createdAt);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Email.Codes.Empty, result.Error.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("invalid@")]
    [InlineData("@example.com")]
    [InlineData("invalid.example.com")]
    public void Register_WithInvalidEmailFormat_ReturnsFailureWithEmailFormatError(string invalidEmail)
    {
        // Arrange
        DateTime createdAt = DateTime.UtcNow;

        // Act
        Result<User> result = User.Register(invalidEmail, "validuser", createdAt);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Email.Codes.InvalidFormat, result.Error.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Register_WithEmptyUserName_ReturnsFailureWithUserNameEmptyError(string? invalidUserName)
    {
        // Arrange
        DateTime createdAt = DateTime.UtcNow;

        // Act
        Result<User> result = User.Register("valid@example.com", invalidUserName!, createdAt);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserName.Codes.Empty, result.Error.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("a")]
    public void Register_WithTooShortUserName_ReturnsFailureWithUserNameTooShortError(string shortUserName)
    {
        // Arrange
        DateTime createdAt = DateTime.UtcNow;

        // Act
        Result<User> result = User.Register("valid@example.com", shortUserName, createdAt);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserName.Codes.TooShort, result.Error.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void Register_WithTooLongUserName_ReturnsFailureWithUserNameTooLongError()
    {
        // Arrange
        string longUserName = new string('a', 51); // 51 characters
        DateTime createdAt = DateTime.UtcNow;

        // Act
        Result<User> result = User.Register("valid@example.com", longUserName, createdAt);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserName.Codes.TooLong, result.Error.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Theory]
    [InlineData("_invalidstart")]
    [InlineData("-invalidstart")]
    [InlineData("invalidend_")]
    [InlineData("invalidend-")]
    [InlineData("invalid spaces")]
    [InlineData("invalid@symbols")]
    [InlineData("invalid#symbols")]
    public void Register_WithInvalidUserNameFormat_ReturnsFailureWithUserNameFormatError(string invalidUserName)
    {
        // Arrange
        DateTime createdAt = DateTime.UtcNow;

        // Act
        Result<User> result = User.Register("valid@example.com", invalidUserName, createdAt);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserName.Codes.InvalidFormat, result.Error.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Theory]
    [InlineData("valid_user")]
    [InlineData("valid-user")]
    [InlineData("valid123")]
    [InlineData("123valid")]
    [InlineData("valid_123-user")]
    public void Register_WithValidUserNameFormats_ReturnsSuccess(string validUserName)
    {
        // Arrange
        DateTime createdAt = DateTime.UtcNow;

        // Act
        Result<User> result = User.Register("valid@example.com", validUserName, createdAt);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(validUserName, result.Value.UserName.Value);
    }

    #endregion

    #region ChangeEmail Method Tests

    [Fact]
    public void ChangeEmail_WithValidNewEmail_UpdatesEmailSuccessfully()
    {
        // Arrange
        User user = new UserBuilder().Build();

        // Act
        Result result = user.ChangeEmail("newemail@example.com");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("newemail@example.com", user.Email.Value.Address);
    }

    [Fact]
    public void ChangeEmail_WithSameEmail_DoesNotUpdateEmail()
    {
        // Arrange
        User user = new UserBuilder().WithEmail("test@example.com").Build();
        Email originalEmailObject = user.Email;

        // Act
        Result result = user.ChangeEmail("test@example.com");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(originalEmailObject, user.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ChangeEmail_WithEmptyEmail_ReturnsFailureWithEmailEmptyError(string? invalidEmail)
    {
        // Arrange
        User user = new UserBuilder().Build();

        // Act
        Result result = user.ChangeEmail(invalidEmail!);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Email.Codes.Empty, result.Error.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("invalid@")]
    [InlineData("@example.com")]
    [InlineData("invalid.example.com")]
    public void ChangeEmail_WithInvalidEmailFormat_ReturnsFailureWithEmailFormatError(string invalidEmail)
    {
        // Arrange
        User user = new UserBuilder().Build();

        // Act
        Result result = user.ChangeEmail(invalidEmail);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Email.Codes.InvalidFormat, result.Error.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void ChangeEmail_WithValidEmail_DoesNotAffectOtherProperties()
    {
        // Arrange
        User user = new UserBuilder().Build();
        UserName originalUserName = user.UserName;
        DateTime originalCreatedAt = user.CreatedAt;
        UserId originalId = user.Id;

        // Act
        Result result = user.ChangeEmail("newemail@example.com");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(originalUserName, user.UserName);
        Assert.Equal(originalCreatedAt, user.CreatedAt);
        Assert.Null(user.LastLoginAt);
        Assert.Equal(originalId, user.Id);
    }

    #endregion

    #region ChangeUserName Method Tests

    [Fact]
    public void ChangeUserName_WithValidNewUserName_UpdatesUserNameSuccessfully()
    {
        // Arrange
        User user = new UserBuilder().Build();

        // Act
        Result result = user.ChangeUserName("newusername");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("newusername", user.UserName.Value);
    }

    [Fact]
    public void ChangeUserName_WithSameUserName_DoesNotUpdateUserName()
    {
        // Arrange
        User user = new UserBuilder().WithUserName("testuser").Build();
        UserName originalUserNameObject = user.UserName;

        // Act
        Result result = user.ChangeUserName("testuser");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(originalUserNameObject, user.UserName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ChangeUserName_WithEmptyUserName_ReturnsFailureWithUserNameEmptyError(string? invalidUserName)
    {
        // Arrange
        User user = new UserBuilder().Build();

        // Act
        Result result = user.ChangeUserName(invalidUserName!);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserName.Codes.Empty, result.Error.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("a")]
    public void ChangeUserName_WithTooShortUserName_ReturnsFailureWithUserNameTooShortError(string shortUserName)
    {
        // Arrange
        User user = new UserBuilder().Build();

        // Act
        Result result = user.ChangeUserName(shortUserName);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserName.Codes.TooShort, result.Error.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void ChangeUserName_WithTooLongUserName_ReturnsFailureWithUserNameTooLongError()
    {
        // Arrange
        User user = new UserBuilder().Build();
        string longUserName = new string('a', 51); // 51 characters

        // Act
        Result result = user.ChangeUserName(longUserName);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserName.Codes.TooLong, result.Error.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Theory]
    [InlineData("_invalidstart")]
    [InlineData("-invalidstart")]
    [InlineData("invalidend_")]
    [InlineData("invalidend-")]
    [InlineData("invalid spaces")]
    [InlineData("invalid@symbols")]
    [InlineData("invalid#symbols")]
    public void ChangeUserName_WithInvalidUserNameFormat_ReturnsFailureWithUserNameFormatError(string invalidUserName)
    {
        // Arrange
        User user = new UserBuilder().Build();

        // Act
        Result result = user.ChangeUserName(invalidUserName);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserName.Codes.InvalidFormat, result.Error.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Theory]
    [InlineData("valid_user")]
    [InlineData("valid-user")]
    [InlineData("valid123")]
    [InlineData("123valid")]
    [InlineData("valid_123-user")]
    public void ChangeUserName_WithValidUserNameFormats_UpdatesSuccessfully(string validUserName)
    {
        // Arrange
        User user = new UserBuilder().Build();

        // Act
        Result result = user.ChangeUserName(validUserName);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(validUserName, user.UserName.Value);
    }

    [Fact]
    public void ChangeUserName_WithValidUserName_DoesNotAffectOtherProperties()
    {
        // Arrange
        User user = new UserBuilder().Build();
        Email originalEmail = user.Email;
        DateTime originalCreatedAt = user.CreatedAt;
        UserId originalId = user.Id;

        // Act
        Result result = user.ChangeUserName("newusername");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(originalEmail, user.Email);
        Assert.Equal(originalCreatedAt, user.CreatedAt);
        Assert.Null(user.LastLoginAt); // Direct assertion instead of storing original
        Assert.Equal(originalId, user.Id);
    }

    #endregion

    #region RecordLogin Method Tests

    [Fact]
    public void RecordLogin_WithValidLoginTime_UpdatesLastLoginAtSuccessfully()
    {
        // Arrange
        User user = new UserBuilder().Build();
        DateTime loginTime = DateTime.UtcNow;

        // Act
        user.RecordLogin(loginTime);

        // Assert
        Assert.Equal(loginTime, user.LastLoginAt);
    }

    [Fact]
    public void RecordLogin_WithValidLoginTime_RaisesUserLoggedInEvent()
    {
        // Arrange
        User user = new UserBuilder().Build();
        DateTime loginTime = DateTime.UtcNow;
        user.ClearDomainEvents(); // Clear events from registration

        // Act
        user.RecordLogin(loginTime);

        // Assert
        IReadOnlyCollection<IDomainEvent> domainEvents = user.GetDomainEvents();
        Assert.Single(domainEvents);

        UserLoggedInEvent userLoggedInEvent = Assert.IsType<UserLoggedInEvent>(domainEvents.First());
        Assert.Equal(user.Id, userLoggedInEvent.UserId);
        Assert.Equal(loginTime, userLoggedInEvent.OccurredOn);
    }

    [Fact]
    public void RecordLogin_CalledMultipleTimes_UpdatesLastLoginAtToLatestValue()
    {
        // Arrange
        User user = new UserBuilder().Build();
        DateTime firstLoginTime = DateTime.UtcNow.AddDays(-1);
        DateTime secondLoginTime = DateTime.UtcNow;

        // Act
        user.RecordLogin(firstLoginTime);
        user.RecordLogin(secondLoginTime);

        // Assert
        Assert.Equal(secondLoginTime, user.LastLoginAt);
    }

    [Fact]
    public void RecordLogin_CalledMultipleTimes_RaisesMultipleUserLoggedInEvents()
    {
        // Arrange
        User user = new UserBuilder().Build();
        DateTime firstLoginTime = DateTime.UtcNow.AddDays(-1);
        DateTime secondLoginTime = DateTime.UtcNow;
        user.ClearDomainEvents(); // Clear events from registration

        // Act
        user.RecordLogin(firstLoginTime);
        user.RecordLogin(secondLoginTime);

        // Assert
        var domainEvents = user.GetDomainEvents().OfType<UserLoggedInEvent>().ToList();
        Assert.Equal(2, domainEvents.Count);

        Assert.Contains(domainEvents, e => e.OccurredOn == firstLoginTime);
        Assert.Contains(domainEvents, e => e.OccurredOn == secondLoginTime);
        Assert.All(domainEvents, e => Assert.Equal(user.Id, e.UserId));
    }

    [Fact]
    public void RecordLogin_WithValidLoginTime_DoesNotAffectOtherProperties()
    {
        // Arrange
        User user = new UserBuilder().Build();
        Email originalEmail = user.Email;
        UserName originalUserName = user.UserName;
        DateTime originalCreatedAt = user.CreatedAt;
        UserId originalId = user.Id;

        // Act
        user.RecordLogin(DateTime.UtcNow);

        // Assert
        Assert.Equal(originalEmail, user.Email);
        Assert.Equal(originalUserName, user.UserName);
        Assert.Equal(originalCreatedAt, user.CreatedAt);
        Assert.Equal(originalId, user.Id);
    }

    [Fact]
    public void RecordLogin_WithPastAndFutureLoginTimes_AcceptsBothValues()
    {
        // Arrange
        User user = new UserBuilder().Build();
        DateTime pastLoginTime = DateTime.UtcNow.AddDays(-30);
        DateTime futureLoginTime = DateTime.UtcNow.AddDays(1);

        // Act & Assert
        user.RecordLogin(pastLoginTime);
        Assert.Equal(pastLoginTime, user.LastLoginAt);

        user.RecordLogin(futureLoginTime);
        Assert.Equal(futureLoginTime, user.LastLoginAt);
    }

    #endregion

    #region Domain Event Tests

    #region UserRegisteredEvent Tests

    [Fact]
    public void UserRegisteredEvent_CreatedByRegister_HasCorrectProperties()
    {
        // Arrange
        DateTime createdAt = DateTime.UtcNow;

        // Act
        Result<User> userResult = User.Register("test@example.com", "testuser", createdAt);
        User user = userResult.Value;
        IReadOnlyCollection<IDomainEvent> domainEvents = user.GetDomainEvents();

        // Assert
        Assert.Single(domainEvents);
        UserRegisteredEvent userRegisteredEvent = Assert.IsType<UserRegisteredEvent>(domainEvents.First());

        Assert.Equal(user.Id, userRegisteredEvent.UserId);
        Assert.Equal("test@example.com", userRegisteredEvent.Email.Value.Address);
        Assert.Equal("testuser", userRegisteredEvent.UserName.Value);
        Assert.Equal(createdAt, userRegisteredEvent.OccurredOn);
    }

    [Fact]
    public void UserRegisteredEvent_WithDifferentEmails_CreatesDistinctEvents()
    {
        // Arrange
        DateTime createdAt = DateTime.UtcNow;

        // Act
        Result<User> user1Result = User.Register("user1@example.com", "testuser", createdAt);
        Result<User> user2Result = User.Register("user2@example.com", "testuser", createdAt);

        UserRegisteredEvent event1 = user1Result.Value.GetDomainEvents().OfType<UserRegisteredEvent>().First();
        UserRegisteredEvent event2 = user2Result.Value.GetDomainEvents().OfType<UserRegisteredEvent>().First();

        // Assert
        Assert.NotEqual(event1.UserId, event2.UserId);
        Assert.Equal("user1@example.com", event1.Email.Value.Address);
        Assert.Equal("user2@example.com", event2.Email.Value.Address);
        Assert.NotEqual(event1, event2);
    }

    [Fact]
    public void UserRegisteredEvent_RecordType_SupportsValueEquality()
    {
        // Arrange
        var userId = UserId.New();
        Email email = new EmailBuilder().Build();
        UserName userName = new UserNameBuilder().Build();
        DateTime occurredOn = DateTime.UtcNow;

        // Act
        var event1 = new UserRegisteredEvent(userId, email, userName, occurredOn);
        var event2 = new UserRegisteredEvent(userId, email, userName, occurredOn);

        // Assert
        Assert.Equal(event1, event2);
        Assert.True(event1 == event2);
        Assert.False(event1 != event2);
        Assert.Equal(event1.GetHashCode(), event2.GetHashCode());
    }

    #endregion

    #region UserLoggedInEvent Tests

    [Fact]
    public void UserLoggedInEvent_CreatedByRecordLogin_HasCorrectProperties()
    {
        // Arrange
        User user = new UserBuilder().Build();
        DateTime loginTime = DateTime.UtcNow;
        user.ClearDomainEvents(); // Clear registration event

        // Act
        user.RecordLogin(loginTime);
        IReadOnlyCollection<IDomainEvent> domainEvents = user.GetDomainEvents();

        // Assert
        Assert.Single(domainEvents);
        UserLoggedInEvent userLoggedInEvent = Assert.IsType<UserLoggedInEvent>(domainEvents.First());

        Assert.Equal(user.Id, userLoggedInEvent.UserId);
        Assert.Equal(loginTime, userLoggedInEvent.OccurredOn);
    }

    [Fact]
    public void UserLoggedInEvent_WithDifferentLoginTimes_CreatesDistinctEvents()
    {
        // Arrange
        User user = new UserBuilder().Build();
        DateTime loginTime1 = DateTime.UtcNow.AddHours(-1);
        DateTime loginTime2 = DateTime.UtcNow;
        user.ClearDomainEvents(); // Clear registration event

        // Act
        user.RecordLogin(loginTime1);
        user.RecordLogin(loginTime2);
        var events = user.GetDomainEvents().OfType<UserLoggedInEvent>().ToList();

        // Assert
        Assert.Equal(2, events.Count);
        Assert.Contains(events, e => e.OccurredOn == loginTime1);
        Assert.Contains(events, e => e.OccurredOn == loginTime2);
        Assert.All(events, e => Assert.Equal(user.Id, e.UserId));
    }

    [Fact]
    public void UserLoggedInEvent_RecordType_SupportsValueEquality()
    {
        // Arrange
        var userId = UserId.New();
        DateTime occurredOn = DateTime.UtcNow;

        // Act
        var event1 = new UserLoggedInEvent(userId, occurredOn);
        var event2 = new UserLoggedInEvent(userId, occurredOn);

        // Assert
        Assert.Equal(event1, event2);
        Assert.True(event1 == event2);
        Assert.False(event1 != event2);
        Assert.Equal(event1.GetHashCode(), event2.GetHashCode());
    }

    [Fact]
    public void UserLoggedInEvent_WithDifferentUserIds_AreNotEqual()
    {
        // Arrange
        var userId1 = UserId.New();
        var userId2 = UserId.New();
        DateTime occurredOn = DateTime.UtcNow;

        // Act
        var event1 = new UserLoggedInEvent(userId1, occurredOn);
        var event2 = new UserLoggedInEvent(userId2, occurredOn);

        // Assert
        Assert.NotEqual(event1, event2);
        Assert.False(event1 == event2);
        Assert.True(event1 != event2);
    }

    #endregion

    #region Domain Event Integration Tests

    [Fact]
    public void User_AfterMultipleOperations_AccumulatesDomainEventsCorrectly()
    {
        // Arrange
        User user = new UserBuilder().Build();
        DateTime loginTime1 = DateTime.UtcNow.AddDays(-1);
        DateTime loginTime2 = DateTime.UtcNow;

        // Act
        user.RecordLogin(loginTime1);
        user.RecordLogin(loginTime2);
        IReadOnlyCollection<IDomainEvent> allEvents = user.GetDomainEvents();

        // Assert
        Assert.Equal(3, allEvents.Count()); // 1 registration + 2 login events

        Assert.Single(allEvents.OfType<UserRegisteredEvent>());
        Assert.Equal(2, allEvents.OfType<UserLoggedInEvent>().Count());
    }

    [Fact]
    public void User_ClearDomainEvents_RemovesAllEvents()
    {
        // Arrange
        User user = new UserBuilder().Build();
        user.RecordLogin(DateTime.UtcNow);

        // Verify events exist
        Assert.NotEmpty(user.GetDomainEvents());

        // Act
        user.ClearDomainEvents();

        // Assert
        Assert.Empty(user.GetDomainEvents());
    }

    [Fact]
    public void User_AfterClearingAndNewOperations_OnlyHasNewEvents()
    {
        // Arrange
        User user = new UserBuilder().Build();
        user.RecordLogin(DateTime.UtcNow.AddDays(-1));
        user.ClearDomainEvents();

        DateTime newLoginTime = DateTime.UtcNow;

        // Act
        user.RecordLogin(newLoginTime);

        // Assert
        IReadOnlyCollection<IDomainEvent> events = user.GetDomainEvents();
        Assert.Single(events);

        UserLoggedInEvent loggedInEvent = Assert.IsType<UserLoggedInEvent>(events.First());
        Assert.Equal(newLoginTime, loggedInEvent.OccurredOn);
    }

    #endregion

    #endregion
}
