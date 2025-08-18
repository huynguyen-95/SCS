using ErrorOr;
using Moq;
using SCS.Api.App.Features.Authentication;
using SCS.Api.App.Helpers;
using DomainUser = SCS.Api.Domain.User;

namespace SCS.Api.UnitTests.Features.Authentication;

public class LoginTests : BaseTest
{
    private readonly Mock<IJwtTokenGenerator> _mockJwtTokenGenerator;
    private readonly Login.Validator _validator;
    private readonly Login.Handler _handler;

    public LoginTests()
    {
        _mockJwtTokenGenerator = new Mock<IJwtTokenGenerator>();
        _validator = new Login.Validator();
        _handler = new Login.Handler(DbContext, _validator, _mockJwtTokenGenerator.Object);
    }

    [Fact]
    public async Task Handle_WhenValidCommandAndUserExists_ShouldReturnAuthenticationResponse()
    {
        // Arrange
        var user = new DomainUser("EMP001", "testuser", false);
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        var command = new Login.Command("EMP001");
        var expectedToken = "test-jwt-token";

        _mockJwtTokenGenerator.Setup(x => x.GenerateToken(It.IsAny<DomainUser>()))
                             .Returns(expectedToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(expectedToken, result.Value.Token);
        _mockJwtTokenGenerator.Verify(x => x.GenerateToken(It.Is<DomainUser>(u => u.EmpNo == "EMP001")), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldReturnNotFoundError()
    {
        // Arrange
        var command = new Login.Command("NONEXISTENT");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        var error = result.FirstError;
        Assert.Equal(ErrorType.NotFound, error.Type);
        Assert.Equal("Login.UserNotFound", error.Code);
        Assert.Equal("User not found.", error.Description);

        _mockJwtTokenGenerator.Verify(x => x.GenerateToken(It.IsAny<DomainUser>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenEmpNoIsEmpty_ShouldReturnValidationError()
    {
        // Arrange
        var command = new Login.Command("");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        var error = result.FirstError;
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal("Login.Validation", error.Code);
        Assert.Equal("Employee number is required.", error.Description);

        _mockJwtTokenGenerator.Verify(x => x.GenerateToken(It.IsAny<DomainUser>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenValidAdminUser_ShouldGenerateTokenCorrectly()
    {
        // Arrange
        var adminUser = new DomainUser("ADM001", "admin", true);
        DbContext.Users.Add(adminUser);
        await DbContext.SaveChangesAsync();

        var command = new Login.Command("ADM001");
        var expectedToken = "admin-jwt-token";

        _mockJwtTokenGenerator.Setup(x => x.GenerateToken(It.IsAny<DomainUser>()))
                             .Returns(expectedToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(expectedToken, result.Value.Token);
        _mockJwtTokenGenerator.Verify(x => x.GenerateToken(It.Is<DomainUser>(u => u.EmpNo == "ADM001" && u.IsAdmin == true)), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenMultipleUsersExist_ShouldReturnCorrectUser()
    {
        // Arrange
        var user1 = new DomainUser("EMP001", "user1", false);
        var user2 = new DomainUser("EMP002", "user2", false);
        var user3 = new DomainUser("EMP003", "user3", true);

        DbContext.Users.AddRange(user1, user2, user3);
        await DbContext.SaveChangesAsync();

        var command = new Login.Command("EMP002");
        var expectedToken = "user2-token";

        _mockJwtTokenGenerator.Setup(x => x.GenerateToken(It.IsAny<DomainUser>()))
                             .Returns(expectedToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(expectedToken, result.Value.Token);
        _mockJwtTokenGenerator.Verify(x => x.GenerateToken(It.Is<DomainUser>(u => u.EmpNo == "EMP002" && u.Username == "user2")), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ShouldRespectCancellationToken()
    {
        // Arrange
        var user = new DomainUser("EMP001", "testuser", false);
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        var command = new Login.Command("EMP001");
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _handler.Handle(command, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task Handle_WhenSpecialCharactersInEmpNo_ShouldFindUserCorrectly()
    {
        // Arrange
        var user = new DomainUser("EMP-特殊@123", "specialuser", false);
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        var command = new Login.Command("EMP-特殊@123");
        var expectedToken = "special-token";

        _mockJwtTokenGenerator.Setup(x => x.GenerateToken(It.IsAny<DomainUser>()))
                             .Returns(expectedToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(expectedToken, result.Value.Token);
        _mockJwtTokenGenerator.Verify(x => x.GenerateToken(It.Is<DomainUser>(u => u.EmpNo == "EMP-特殊@123")), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDatabaseIsEmpty_ShouldReturnNotFoundError()
    {
        // Arrange
        var command = new Login.Command("EMP001");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        var error = result.FirstError;
        Assert.Equal(ErrorType.NotFound, error.Type);
        Assert.Equal("Login.UserNotFound", error.Code);

        _mockJwtTokenGenerator.Verify(x => x.GenerateToken(It.IsAny<DomainUser>()), Times.Never);
    }

    [Fact]
    public async Task Validator_WhenValidEmpNo_ShouldPassValidation()
    {
        // Arrange
        var command = new Login.Command("EMP001");

        // Act
        var validationResult = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(validationResult.IsValid);
        Assert.Empty(validationResult.Errors);
    }

    [Fact]
    public async Task Validator_WhenEmpNoIsEmpty_ShouldFailValidation()
    {
        // Arrange
        var command = new Login.Command("");

        // Act
        var validationResult = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.Single(validationResult.Errors);
        Assert.Equal("Employee number is required.", validationResult.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task Validator_WhenEmpNoIsNull_ShouldFailValidation()
    {
        // Arrange
        var command = new Login.Command(null!);

        // Act
        var validationResult = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.Single(validationResult.Errors);
        Assert.Equal("Employee number is required.", validationResult.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task Validator_WhenEmpNoIsWhitespace_ShouldFailValidation()
    {
        // Arrange
        var command = new Login.Command("   ");

        // Act
        var validationResult = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.Single(validationResult.Errors);
        Assert.Equal("Employee number is required.", validationResult.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task Validator_WhenEmpNoHasSpecialCharacters_ShouldPassValidation()
    {
        // Arrange
        var command = new Login.Command("EMP-001@company.com");

        // Act
        var validationResult = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(validationResult.IsValid);
        Assert.Empty(validationResult.Errors);
    }

    [Fact]
    public async Task Handle_WhenJwtTokenGeneratorThrowsException_ShouldPropagateException()
    {
        // Arrange
        var user = new DomainUser("EMP001", "testuser", false);
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        var command = new Login.Command("EMP001");
        var expectedException = new Exception("Token generation failed");

        _mockJwtTokenGenerator.Setup(x => x.GenerateToken(It.IsAny<DomainUser>()))
                             .Throws(expectedException);

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<Exception>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal(expectedException.Message, actualException.Message);
    }
}
