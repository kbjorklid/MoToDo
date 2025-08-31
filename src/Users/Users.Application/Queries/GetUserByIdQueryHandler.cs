using Base.Domain.Result;
using Users.Contracts;
using Users.Domain;

namespace Users.Application.Queries;

/// <summary>
/// Handles the GetUserByIdQuery to retrieve users by their unique identifier.
/// </summary>
public static class GetUserByIdQueryHandler
{
    /// <summary>
    /// Handles the query to retrieve a user by ID using Wolverine's preferred static method pattern.
    /// </summary>
    /// <param name="query">The get user by ID query containing the user identifier.</param>
    /// <param name="userRepository">The user repository injected by Wolverine.</param>
    /// <returns>A Result containing the GetUserByIdResult if successful, or an error if user not found or validation fails.</returns>
    public static async Task<Result<UserDto>> Handle(GetUserByIdQuery query, IUserRepository userRepository)
    {
        Result<UserId> userIdResult = UserId.FromString(query.UserId);
        if (userIdResult.IsFailure)
        {
            return userIdResult.Error;
        }

        UserId userId = userIdResult.Value;

        User? user = await userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            return new Error(User.Codes.NotFound, "User not found.", ErrorType.NotFound);
        }

        return new UserDto(
            user.Id.Value,
            user.Email.Value.Address,
            user.UserName.Value,
            user.CreatedAt,
            user.LastLoginAt
        );
    }
}
