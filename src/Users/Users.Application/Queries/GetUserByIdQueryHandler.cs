using Base.Domain.Result;
using Microsoft.EntityFrameworkCore;
using Users.Application.Ports;
using Users.Contracts;
using Users.Domain;

namespace Users.Application.Queries;

/// <summary>
/// Handles the GetUserByIdQuery to retrieve users by their unique identifier.
/// </summary>
public static class GetUserByIdQueryHandler
{
    /// <summary>
    /// Handles the query to retrieve a user by ID using CQRS principles - direct database querying via query context.
    /// </summary>
    /// <param name="query">The get user by ID query containing the user identifier.</param>
    /// <param name="queryContext">The query context for direct database access.</param>
    /// <returns>A Result containing the UserDto if successful, or an error if user not found or validation fails.</returns>
    public static async Task<Result<UserDto>> Handle(GetUserByIdQuery query, IUsersQueryContext queryContext)
    {
        Result<UserId> userIdResult = UserId.FromString(query.UserId);
        if (userIdResult.IsFailure)
        {
            return userIdResult.Error;
        }

        UserId userId = userIdResult.Value;

        UserDto? userDto = await queryContext.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new UserDto(
                u.Id.Value,
                u.Email.Value.Address,
                u.UserName.Value,
                u.CreatedAt,
                u.LastLoginAt
            ))
            .FirstOrDefaultAsync();

        if (userDto is null)
        {
            return new Error(User.Codes.NotFound, "User not found.", ErrorType.NotFound);
        }

        return userDto;
    }
}
