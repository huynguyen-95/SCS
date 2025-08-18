using Amazon.SQS;
using Amazon.SQS.Model;
using ErrorOr;
using FluentValidation;
using Microsoft.Extensions.Options;
using Moq;
using SCS.Api.App.Abstraction.Messaging;
using SCS.Api.App.Features.AlarmSystem;
using SCS.Api.App.Settings;
using System.Net;

namespace SCS.Api.UnitTests.Features.AlarmSystem;

public class SimulateAlarmSystemAlertTests
{
    private readonly Mock<IAmazonSQS> _mockSqsClient;
    private readonly Mock<IOptions<AwsOptions>> _mockOptions;
    private readonly SimulateAlarmSystemAlert.Validator _validator;
    private readonly AwsOptions _awsOptions;
    private readonly SimulateAlarmSystemAlert.Handler _handler;

    public SimulateAlarmSystemAlertTests()
    {
        _mockSqsClient = new Mock<IAmazonSQS>();
        _mockOptions = new Mock<IOptions<AwsOptions>>();
        _validator = new SimulateAlarmSystemAlert.Validator();

        _awsOptions = new AwsOptions
        {
            QueueUrl = "https://sqs.us-east-1.amazonaws.com/123456789/test-queue",
            BucketName = "test-bucket",
            Region = "us-east-1",
            AccessKey = "test-access-key",
            SecretKey = "test-secret-key"
        };

        _mockOptions.Setup(x => x.Value).Returns(_awsOptions);
        _handler = new SimulateAlarmSystemAlert.Handler(_validator, _mockOptions.Object, _mockSqsClient.Object);
    }

    [Fact]
    public void Constructor_WhenValidatorIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new SimulateAlarmSystemAlert.Handler(null!, _mockOptions.Object, _mockSqsClient.Object));
        Assert.Equal("validator", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenAwsOptionsIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new SimulateAlarmSystemAlert.Handler(_validator, null!, _mockSqsClient.Object));
        Assert.Equal("awsOptions", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenSqsClientIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new SimulateAlarmSystemAlert.Handler(_validator, _mockOptions.Object, null!));
        Assert.Equal("sqsClient", exception.ParamName);
    }

    [Fact]
    public async Task Handle_WhenValidCommand_ShouldCallSendMessageAsync()
    {
        // Arrange
        var command = new SimulateAlarmSystemAlert.Command(1, "Test alarm message");

        _mockSqsClient.Setup(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new SendMessageResponse { HttpStatusCode = HttpStatusCode.OK });

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockSqsClient.Verify(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var command = new SimulateAlarmSystemAlert.Command(1, "Test alarm message");

        _mockSqsClient.Setup(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new SendMessageResponse { HttpStatusCode = HttpStatusCode.OK });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(Unit.Value, result.Value);
    }

    [Fact]
    public async Task Handle_WhenValidCommand_ShouldPassCorrectMessageRequest()
    {
        // Arrange
        var command = new SimulateAlarmSystemAlert.Command(123, "Fire alarm detected");
        SendMessageRequest? capturedRequest = null;

        _mockSqsClient.Setup(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
                     .Callback<SendMessageRequest, CancellationToken>((req, token) => capturedRequest = req)
                     .ReturnsAsync(new SendMessageResponse { HttpStatusCode = HttpStatusCode.OK });

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal(_awsOptions.QueueUrl, capturedRequest.QueueUrl);
        Assert.Contains("123", capturedRequest.MessageBody);
        Assert.Contains("Fire alarm detected", capturedRequest.MessageBody);
    }

    [Fact]
    public async Task Handle_WhenPremiseIdIsZero_ShouldReturnValidationError()
    {
        // Arrange
        var command = new SimulateAlarmSystemAlert.Command(0, "Test message");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        var error = result.FirstError;
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal("SimulateAlarmSystemAlert.Validation", error.Code);

        _mockSqsClient.Verify(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPremiseIdIsNegative_ShouldReturnValidationError()
    {
        // Arrange
        var command = new SimulateAlarmSystemAlert.Command(-5, "Test message");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        var error = result.FirstError;
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal("SimulateAlarmSystemAlert.Validation", error.Code);

        _mockSqsClient.Verify(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenMessageIsEmpty_ShouldReturnValidationError()
    {
        // Arrange
        var command = new SimulateAlarmSystemAlert.Command(1, "");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        var error = result.FirstError;
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal("SimulateAlarmSystemAlert.Validation", error.Code);

        _mockSqsClient.Verify(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSqsReturnsNonOkStatus_ShouldReturnFailureError()
    {
        // Arrange
        var command = new SimulateAlarmSystemAlert.Command(1, "Test message");

        _mockSqsClient.Setup(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new SendMessageResponse { HttpStatusCode = HttpStatusCode.InternalServerError });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        var error = result.FirstError;
        Assert.Equal(ErrorType.Failure, error.Type);
        Assert.Equal("SimulateAlarmSystemAlert.Failure", error.Code);
        Assert.Equal("Failed to send message to SQS", error.Description);
    }

    [Fact]
    public async Task Handle_WhenSqsThrowsException_ShouldPropagateException()
    {
        // Arrange
        var command = new SimulateAlarmSystemAlert.Command(1, "Test message");
        var expectedException = new Exception("SQS Error");

        _mockSqsClient.Setup(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(expectedException);

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<Exception>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal(expectedException.Message, actualException.Message);
        _mockSqsClient.Verify(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithSpecialCharacters_ShouldCallSendMessageAsync()
    {
        // Arrange
        var command = new SimulateAlarmSystemAlert.Command(1, "Fire detected! 🔥 Location: 北京大厦");

        _mockSqsClient.Setup(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new SendMessageResponse { HttpStatusCode = HttpStatusCode.OK });

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockSqsClient.Verify(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToSqsClient()
    {
        // Arrange
        var command = new SimulateAlarmSystemAlert.Command(1, "Test message");
        var cancellationToken = new CancellationTokenSource().Token;

        _mockSqsClient.Setup(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), cancellationToken))
                     .ReturnsAsync(new SendMessageResponse { HttpStatusCode = HttpStatusCode.OK });

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _mockSqsClient.Verify(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Validator_WhenValidCommand_ShouldPassValidation()
    {
        // Arrange
        var command = new SimulateAlarmSystemAlert.Command(1, "Valid message");

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
        var command = new SimulateAlarmSystemAlert.Command(0, "Valid message");

        // Act
        var validationResult = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.Single(validationResult.Errors);
        Assert.Equal("Premise ID must be greater than 0.", validationResult.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task Validator_WhenMessageIsEmpty_ShouldFailValidation()
    {
        // Arrange
        var command = new SimulateAlarmSystemAlert.Command(1, "");

        // Act
        var validationResult = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.Single(validationResult.Errors);
        Assert.Equal("Message cannot be empty.", validationResult.Errors[0].ErrorMessage);
    }

    [Fact]
    public void Command_ShouldImplementIRequest()
    {
        // Arrange & Act
        var command = new SimulateAlarmSystemAlert.Command(1, "Test message");

        // Assert
        Assert.IsAssignableFrom<IRequest<ErrorOr<Unit>>>(command);
        Assert.Equal(1, command.PremiseId);
        Assert.Equal("Test message", command.Message);
    }

    [Fact]
    public void Handler_ShouldImplementIRequestHandler()
    {
        // Arrange & Act
        var handler = new SimulateAlarmSystemAlert.Handler(_validator, _mockOptions.Object, _mockSqsClient.Object);

        // Assert
        Assert.IsAssignableFrom<IRequestHandler<SimulateAlarmSystemAlert.Command, ErrorOr<Unit>>>(handler);
    }
}
