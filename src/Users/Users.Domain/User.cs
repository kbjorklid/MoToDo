using Base.Domain;
using Base.Domain.Result;

namespace Users.Domain;

/// <summary>
/// The User aggregate root represents a system user with their basic identity and authentication information 
/// within the MoToDo task management system.
/// </summary>
public sealed class User : AggregateRoot<UserId>
{
    public static class Codes
    {
        public const string EmailAlreadyInUse = "User.EmailAlreadyInUse";
        public const string UserNameAlreadyInUse = "User.UserNameAlreadyInUse";
        public const string NotFound = "User.NotFound";
    }

    public Email Email { get; private set; }
    public UserName UserName { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    private User(UserId id, Email email, UserName userName, DateTime createdAt, DateTime? lastLoginAt) : base(id)
    {
        Email = email;
        UserName = userName;
        CreatedAt = createdAt;
        LastLoginAt = lastLoginAt;
    }

    /// <summary>
    /// Registers a new user in the system.
    /// </summary>
    /// <param name="emailValue">The user's email address string.</param>
    /// <param name="userNameValue">The user's chosen username string.</param>
    /// <param name="createdAt">The date and time when the user was created.</param>
    /// <returns>A Result containing the new User instance if successful, or an error message if validation fails.</returns>
    public static Result<User> Register(string emailValue, string userNameValue, DateTime createdAt)
    {
        Result<Email> emailResult = Email.Create(emailValue);
        if (emailResult.IsFailure)
            return emailResult.Error;

        Result<UserName> userNameResult = UserName.Create(userNameValue);
        if (userNameResult.IsFailure)
            return userNameResult.Error;

        var userId = UserId.New();
        var user = new User(userId, emailResult.Value, userNameResult.Value, createdAt, null);
        user.AddDomainEvent(new UserRegisteredEvent(userId, emailResult.Value, userNameResult.Value, createdAt));

        return user;
    }

    /// <summary>
    /// Changes the user's email address.
    /// </summary>
    /// <param name="emailValue">The new email address string.</param>
    /// <returns>A Result indicating success or failure with error message.</returns>
    public Result ChangeEmail(string emailValue)
    {
        Result<Email> emailResult = Email.Create(emailValue);
        if (emailResult.IsFailure)
            return emailResult.Error;

        Email newEmail = emailResult.Value;
        if (Email.Value.Address != newEmail.Value.Address)
            Email = newEmail;

        return Result.Success();
    }

    /// <summary>
    /// Changes the user's username.
    /// </summary>
    /// <param name="userNameValue">The new username string.</param>
    /// <returns>A Result indicating success or failure with error message.</returns>
    public Result ChangeUserName(string userNameValue)
    {
        Result<UserName> userNameResult = UserName.Create(userNameValue);
        if (userNameResult.IsFailure)
            return userNameResult.Error;

        if (UserName.Value != userNameResult.Value)
            UserName = userNameResult.Value;

        return Result.Success();
    }

    /// <summary>
    /// Records that the user has successfully logged in.
    /// </summary>
    /// <param name="loginTime">The date and time when the login occurred.</param>
    public void RecordLogin(DateTime loginTime)
    {
        LastLoginAt = loginTime;
        AddDomainEvent(new UserLoggedInEvent(Id, loginTime));
    }

    /// <summary>
    /// Marks the user for deletion and raises appropriate domain events.
    /// </summary>
    /// <param name="deletedAt">The date and time when the deletion occurred.</param>
    public void MarkForDeletion(DateTime deletedAt)
    {
        AddDomainEvent(new UserDeletedEvent(Id, Email, UserName, deletedAt));
    }
}
