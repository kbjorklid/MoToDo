using Base.Domain.Result;
using Users.Contracts;
using Users.Domain;

namespace Users.Application.Commands;

/// <summary>
/// Handles the DeleteUserCommand to remove users from the system.
/// </summary>
public static class DeleteUserCommandHandler
{
    /// <summary>
    /// Handles the command to delete a user using Wolverine's preferred static method pattern.
    /// </summary>
    /// <param name="command">The delete user command containing the user ID.</param>
    /// <param name="userRepository">The user repository</param>
    /// <param name="timeProvider">The time provider</param>
    /// <returns>A Result indicating success or failure of the deletion operation.</returns>
    public static async Task<Result> Handle(DeleteUserCommand command, IUserRepository userRepository, TimeProvider timeProvider)
    {
        Result<UserId> userIdResult = UserId.FromString(command.UserId);
        if (userIdResult.IsFailure)
        {
            return userIdResult.Error;
        }

        UserId userId = userIdResult.Value;

        User? user = await userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            return new Error(
                User.Codes.NotFound,
                $"User with ID '{userId}' was not found.",
                ErrorType.NotFound);
        }

        DateTime now = timeProvider.GetUtcNow().UtcDateTime;
        user.MarkForDeletion(now);

        Result deleteResult = await userRepository.DeleteAsync(userId);
        if (deleteResult.IsFailure)
        {
            return deleteResult.Error;
        }

        await userRepository.SaveChangesAsync();

        return Result.Success();
    }
}
