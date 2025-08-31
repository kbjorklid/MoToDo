using Base.Domain.Result;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Users.Application.Commands;
using Users.Contracts;
using Users.Domain;

namespace Users.Application.Tests.Commands;

public class AddUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly TimeProvider _timeProvider = new FakeTimeProvider();

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessResult()
    {
        // Arrange
        var command = new AddUserCommand("test@example.com", "testuser");
        _userRepository.GetByEmailAsync(Arg.Any<Email>()).Returns((User?)null);
        _userRepository.GetByUserNameAsync(Arg.Any<UserName>()).Returns((User?)null);

        // Act
        Result<AddUserResult> result = await AddUserCommandHandler.Handle(command, _userRepository, _timeProvider);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("test@example.com", result.Value.Email);
        Assert.Equal("testuser", result.Value.UserName);

        await _userRepository.Received(1).AddAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_EmailAlreadyExists_ReturnsValidationError()
    {
        // Arrange
        var command = new AddUserCommand("test@example.com", "testuser");
        User existingUser = User.Register("test@example.com", "existinguser", DateTime.UtcNow).Value;
        _userRepository.GetByEmailAsync(Arg.Any<Email>()).Returns(existingUser);

        // Act
        Result<AddUserResult> result = await AddUserCommandHandler.Handle(command, _userRepository, _timeProvider);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(User.Codes.EmailAlreadyInUse, result.Error.Code);

        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_UserNameAlreadyExists_ReturnsValidationError()
    {
        // Arrange
        var command = new AddUserCommand("test@example.com", "testuser");
        User existingUser = User.Register("existing@example.com", "testuser", DateTime.UtcNow).Value;
        _userRepository.GetByEmailAsync(Arg.Any<Email>()).Returns((User?)null);
        _userRepository.GetByUserNameAsync(Arg.Any<UserName>()).Returns(existingUser);

        // Act
        Result<AddUserResult> result = await AddUserCommandHandler.Handle(command, _userRepository, _timeProvider);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(User.Codes.UserNameAlreadyInUse, result.Error.Code);

        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_InvalidEmail_ReturnsValidationError()
    {
        // Arrange
        var command = new AddUserCommand("invalid-email", "testuser");

        // Act
        Result<AddUserResult> result = await AddUserCommandHandler.Handle(command, _userRepository, _timeProvider);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);

        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_InvalidUserName_ReturnsValidationError()
    {
        // Arrange
        var command = new AddUserCommand("test@example.com", ""); // Empty username

        // Act
        Result<AddUserResult> result = await AddUserCommandHandler.Handle(command, _userRepository, _timeProvider);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);

        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>());
    }
}
