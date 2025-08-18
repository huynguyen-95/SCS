using ErrorOr;
using Moq;
using SCS.Api.App.Abstraction.Messaging;
using SCS.Api.App.Features.SecurityGuard;
using SCS.Api.App.Services;

namespace SCS.Api.UnitTests.Features.SecurityGuard;

public class DispatchToPremiseTests
{
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly DispatchToPremise.Validator _validator;
    private readonly DispatchToPremise.Handler _handler;

    public DispatchToPremiseTests()
    {
        _mockEmailService = new Mock<IEmailService>();
        _validator = new DispatchToPremise.Validator();
        _handler = new DispatchToPremise.Handler(_mockEmailService.Object, _validator);
    }

    [Fact]
    public void Constructor_WhenEmailServiceIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new DispatchToPremise.Handler(null!, _validator));
        Assert.Equal("emailService", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenValidatorIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new DispatchToPremise.Handler(_mockEmailService.Object, null!));
        Assert.Equal("validator", exception.ParamName);
    }

    [Fact]
    public async Task Handle_WhenValidCommand_ShouldCallEmailServiceAndReturnSuccess()
    {
        // Arrange
        var command = new DispatchToPremise.Command(1, "guard@example.com");

        _mockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                         .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(Unit.Value, result.Value);

        _mockEmailService.Verify(x => x.SendEmailAsync(
            "guard@example.com",
            "Dispatch Notification",
            "You have been dispatched to premise ID 1"), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPremiseIdIsZero_ShouldReturnValidationError()
    {
        // Arrange
        var command = new DispatchToPremise.Command(0, "guard@example.com");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        var error = result.FirstError;
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal("DispatchToPremise.Validation", error.Code);
        Assert.Equal("Validation failed", error.Description);

        _mockEmailService.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPremiseIdIsNegative_ShouldReturnValidationError()
    {
        // Arrange
        var command = new DispatchToPremise.Command(-5, "guard@example.com");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        var error = result.FirstError;
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal("DispatchToPremise.Validation", error.Code);

        _mockEmailService.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenGuardEmailIsEmpty_ShouldReturnValidationError()
    {
        // Arrange
        var command = new DispatchToPremise.Command(1, "");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        var error = result.FirstError;
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal("DispatchToPremise.Validation", error.Code);

        _mockEmailService.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenGuardEmailIsInvalid_ShouldReturnValidationError()
    {
        // Arrange
        var command = new DispatchToPremise.Command(1, "invalid-email");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        var error = result.FirstError;
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal("DispatchToPremise.Validation", error.Code);

        _mockEmailService.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenEmailServiceThrowsException_ShouldPropagateException()
    {
        // Arrange
        var command = new DispatchToPremise.Command(1, "guard@example.com");
        var expectedException = new Exception("Email service error");

        _mockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                         .ThrowsAsync(expectedException);

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<Exception>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal(expectedException.Message, actualException.Message);
        _mockEmailService.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenValidCommandWithSpecialCharacters_ShouldCallEmailService()
    {
        // Arrange
        var command = new DispatchToPremise.Command(999, "guard+test@example-company.com");

        _mockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                         .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(Unit.Value, result.Value);

        _mockEmailService.Verify(x => x.SendEmailAsync(
            "guard+test@example-company.com",
            "Dispatch Notification",
            "You have been dispatched to premise ID 999"), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ShouldPassCancellationTokenToEmailService()
    {
        // Arrange
        var command = new DispatchToPremise.Command(1, "guard@example.com");
        var cancellationTokenSource = new CancellationTokenSource();

        _mockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                         .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, cancellationTokenSource.Token);

        // Assert
        _mockEmailService.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Validator_WhenValidCommand_ShouldPassValidation()
    {
        // Arrange
        var command = new DispatchToPremise.Command(1, "guard@example.com");

        // Act
        var validationResult = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(validationResult.IsValid);
        Assert.Empty(validationResult.Errors);
    }

    [Fact]
    public async Task Validator_WhenPremiseIdIsZero_ShouldFailValidation()
    {
        // Arrange
        var command = new DispatchToPremise.Command(0, "guard@example.com");

        // Act
        var validationResult = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.Single(validationResult.Errors);
        Assert.Equal("Premise ID must be greater than 0.", validationResult.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task Validator_WhenPremiseIdIsNegative_ShouldFailValidation()
    {
        // Arrange
        var command = new DispatchToPremise.Command(-1, "guard@example.com");

        // Act
        var validationResult = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.Single(validationResult.Errors);
        Assert.Equal("Premise ID must be greater than 0.", validationResult.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task Validator_WhenGuardEmailIsEmpty_ShouldFailValidation()
    {
        // Arrange
        var command = new DispatchToPremise.Command(1, "");

        // Act
        var validationResult = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.True(validationResult.Errors.Count >= 1);
        Assert.Contains(validationResult.Errors, e => e.ErrorMessage.Contains("must not be empty") || e.ErrorMessage.Contains("valid email address"));
    }

    [Fact]
    public async Task Validator_WhenGuardEmailIsNull_ShouldFailValidation()
    {
        // Arrange
        var command = new DispatchToPremise.Command(1, null!);

        // Act
        var validationResult = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.True(validationResult.Errors.Count >= 1);
        Assert.Contains(validationResult.Errors, e => e.ErrorMessage.Contains("must not be empty") || e.ErrorMessage.Contains("valid email address"));
    }

    [Fact]
    public async Task Validator_WhenGuardEmailIsInvalidFormat_ShouldFailValidation()
    {
        // Arrange
        var command = new DispatchToPremise.Command(1, "invalid-email-format");

        // Act
        var validationResult = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.Single(validationResult.Errors);
        Assert.Equal("Guard email must be a valid email address.", validationResult.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task Validator_WhenGuardEmailHasValidFormat_ShouldPassValidation()
    {
        // Arrange
        var command = new DispatchToPremise.Command(1, "valid.email+test@example-company.com");

        // Act
        var validationResult = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(validationResult.IsValid);
        Assert.Empty(validationResult.Errors);
    }

    [Fact]
    public async Task Validator_WhenBothFieldsAreInvalid_ShouldReturnMultipleErrors()
    {
        // Arrange
        var command = new DispatchToPremise.Command(0, "invalid-email");

        // Act
        var validationResult = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.Equal(2, validationResult.Errors.Count);
        Assert.Contains(validationResult.Errors, e => e.ErrorMessage == "Premise ID must be greater than 0.");
        Assert.Contains(validationResult.Errors, e => e.ErrorMessage == "Guard email must be a valid email address.");
    }

    [Fact]
    public async Task Handle_WhenValidCommand_ShouldUseCorrectEmailParameters()
    {
        // Arrange
        var command = new DispatchToPremise.Command(123, "security@company.com");
        string? capturedEmail = null;
        string? capturedSubject = null;
        string? capturedBody = null;

        _mockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                         .Callback<string, string, string>((email, subject, body) =>
                         {
                             capturedEmail = email;
                             capturedSubject = subject;
                             capturedBody = body;
                         })
                         .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("security@company.com", capturedEmail);
        Assert.Equal("Dispatch Notification", capturedSubject);
        Assert.Equal("You have been dispatched to premise ID 123", capturedBody);
    }
}
