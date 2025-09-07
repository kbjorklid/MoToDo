using Base.Domain.Result;
using Users.Contracts;
using Users.Domain;

namespace Users.Application.Commands;

/// <summary>
/// Handles the AddUserCommand to create new users in the system.
/// </summary>
public static class AddUserCommandHandler
{
    /// <summary>
    /// Handles the command to add a new user using Wolverine's preferred static method pattern.
    /// </summary>
    /// <param name="command">The add user command containing email and username.</param>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="timeProvider">The time provider.</param>
    /// <returns>A Result containing the AddUserResult if successful, or an error if validation fails.</returns>
    public static async Task<Result<AddUserResult>> Handle(AddUserCommand command, IUserRepository userRepository, TimeProvider timeProvider)
    {
        DateTime now = timeProvider.GetUtcNow().UtcDateTime;
        Result<User> userResult = User.Register(command.Email, command.UserName, now);
        if (userResult.IsFailure)
        {
            return userResult.Error;
        }

        User user = userResult.Value;

        Result uniquenessValidation = await ValidateUserUniqueness(userRepository, user);
        if (uniquenessValidation.IsFailure)
            return uniquenessValidation.Error;

        await userRepository.AddAsync(user);
        await userRepository.SaveChangesAsync();

        return new AddUserResult(
            user.Id.Value,
            user.Email.Value.Address,
            user.UserName.Value,
            user.CreatedAt
        );
    }

    private static async Task<Result> ValidateUserUniqueness(IUserRepository userRepository, User user)
    {
        User? existingEmailUser = await userRepository.GetByEmailAsync(user.Email);
        if (existingEmailUser is not null)
            return new Error(User.Codes.EmailAlreadyInUse, "A user with this email address already exists.", ErrorType.Validation);

        User? existingUserNameUser = await userRepository.GetByUserNameAsync(user.UserName);
        if (existingUserNameUser is not null)
            return new Error(User.Codes.UserNameAlreadyInUse, "A user with this username already exists.", ErrorType.Validation);

        return Result.Success();
    }
}
