using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Moq;
using SCS.Api.App.Abstraction.Messaging;
using SCS.Api.App.Features.SecurityGuard;
using SCS.Api.App.Messaging;
using SCS.Api.App.Services;
using SCS.Api.Domain;

namespace SCS.Api.UnitTests.Features.SecurityGuard;

public class CaptureIncidentTests : BaseTest
{
    private readonly Mock<IUploadFileService> _mockUploadFileService;
    private readonly Mock<ICurrentUserAccessor> _mockCurrentUserAccessor;
    private readonly Mock<IHubContext<AlarmSystemHub>> _mockHubContext;
    private readonly CaptureIncident.Validator _validator;
    private readonly CaptureIncident.Handler _handler;

    public CaptureIncidentTests()
    {
        _mockUploadFileService = new Mock<IUploadFileService>();
        _mockCurrentUserAccessor = new Mock<ICurrentUserAccessor>();
        _mockHubContext = new Mock<IHubContext<AlarmSystemHub>>();
        _validator = new CaptureIncident.Validator();

        // Setup SignalR mocking chain properly
        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();

        _mockHubContext.Setup(x => x.Clients).Returns(mockClients.Object);
        mockClients.Setup(x => x.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);

        _handler = new CaptureIncident.Handler(
            DbContext,
            _mockUploadFileService.Object,
            _mockCurrentUserAccessor.Object,
            _mockHubContext.Object,
            _validator);
    }

    [Fact]
    public async Task Handle_WhenValidCommand_ShouldCreateIncidentAndReturnSuccess()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("test.jpg");
        mockFile.Setup(f => f.Length).Returns(1024);

        var command = new CaptureIncident.Command(1, mockFile.Object, "Test incident", DateTimeOffset.Now);
        var uploadedFilePath = "uploads/test.jpg";
        var userEmpNo = "EMP001";

        _mockUploadFileService.Setup(x => x.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                             .ReturnsAsync(uploadedFilePath);
        _mockCurrentUserAccessor.Setup(x => x.GetUserEmpNo()).Returns(userEmpNo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(Unit.Value, result.Value);

        var incident = DbContext.Incidents.FirstOrDefault();
        Assert.NotNull(incident);
        Assert.Equal(1, incident.PremiseId);
        Assert.Equal("Test incident", incident.Description);
        Assert.Equal(uploadedFilePath, incident.FilePath);
        Assert.Equal(userEmpNo, incident.CreatedBy);

        _mockUploadFileService.Verify(x => x.UploadFileAsync(mockFile.Object, It.IsAny<CancellationToken>()), Times.Once);
        _mockCurrentUserAccessor.Verify(x => x.GetUserEmpNo(), Times.Once);
        _mockHubContext.Verify(x => x.Clients, Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPremiseIdIsZero_ShouldReturnValidationError()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var command = new CaptureIncident.Command(0, mockFile.Object, "Test incident", DateTimeOffset.Now);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        var error = result.FirstError;
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal("CaptureIncident.Validation", error.Code);
        Assert.Equal("validation failed", error.Description);

        _mockUploadFileService.Verify(x => x.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.Empty(DbContext.Incidents);
    }

    [Fact]
    public async Task Handle_WhenFileIsNull_ShouldReturnValidationError()
    {
        // Arrange
        var command = new CaptureIncident.Command(1, null!, "Test incident", DateTimeOffset.Now);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        var error = result.FirstError;
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal("CaptureIncident.Validation", error.Code);

        _mockUploadFileService.Verify(x => x.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.Empty(DbContext.Incidents);
    }

    [Fact]
    public async Task Handle_WhenDescriptionIsEmpty_ShouldReturnValidationError()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var command = new CaptureIncident.Command(1, mockFile.Object, "", DateTimeOffset.Now);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        var error = result.FirstError;
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal("CaptureIncident.Validation", error.Code);

        _mockUploadFileService.Verify(x => x.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.Empty(DbContext.Incidents);
    }

    [Fact]
    public async Task Handle_WhenFileUploadFails_ShouldReturnFailureError()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var command = new CaptureIncident.Command(1, mockFile.Object, "Test incident", DateTimeOffset.Now);

        _mockUploadFileService.Setup(x => x.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                             .ReturnsAsync(Error.Failure("Upload.Failed", "Upload failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        var error = result.FirstError;
        Assert.Equal(ErrorType.Failure, error.Type);
        Assert.Equal("CaptureIncident.UploadFailed", error.Code);
        Assert.Equal("Failed to upload file.", error.Description);

        _mockUploadFileService.Verify(x => x.UploadFileAsync(mockFile.Object, It.IsAny<CancellationToken>()), Times.Once);
        Assert.Empty(DbContext.Incidents);
    }

    [Fact]
    public async Task Handle_WhenValidCommand_ShouldSendSignalRNotification()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var incidentDate = DateTimeOffset.Now;
        var command = new CaptureIncident.Command(123, mockFile.Object, "Security breach", incidentDate);
        var uploadedFilePath = "uploads/security.jpg";
        var userEmpNo = "EMP001";

        _mockUploadFileService.Setup(x => x.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                             .ReturnsAsync(uploadedFilePath);
        _mockCurrentUserAccessor.Setup(x => x.GetUserEmpNo()).Returns(userEmpNo);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockHubContext.Verify(x => x.Clients, Times.Once);
    }

    [Fact]
    public async Task Handle_WhenMultipleIncidentsForSamePremise_ShouldCreateBothIncidents()
    {
        // Arrange
        var mockFile1 = new Mock<IFormFile>();
        var mockFile2 = new Mock<IFormFile>();

        var command1 = new CaptureIncident.Command(1, mockFile1.Object, "First incident", DateTimeOffset.Now);
        var command2 = new CaptureIncident.Command(1, mockFile2.Object, "Second incident", DateTimeOffset.Now.AddMinutes(30));

        _mockUploadFileService.Setup(x => x.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                             .ReturnsAsync("uploads/file.jpg");
        _mockCurrentUserAccessor.Setup(x => x.GetUserEmpNo()).Returns("EMP001");

        // Act
        await _handler.Handle(command1, CancellationToken.None);
        await _handler.Handle(command2, CancellationToken.None);

        // Assert
        var incidents = DbContext.Incidents.Where(i => i.PremiseId == 1).ToList();
        Assert.Equal(2, incidents.Count);
        Assert.Contains(incidents, i => i.Description == "First incident");
        Assert.Contains(incidents, i => i.Description == "Second incident");
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ShouldRespectCancellationToken()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var command = new CaptureIncident.Command(1, mockFile.Object, "Test incident", DateTimeOffset.Now);
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _handler.Handle(command, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task Handle_WhenUploadFileServiceThrowsException_ShouldPropagateException()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var command = new CaptureIncident.Command(1, mockFile.Object, "Test incident", DateTimeOffset.Now);
        var expectedException = new Exception("Upload service error");

        _mockUploadFileService.Setup(x => x.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                             .ThrowsAsync(expectedException);

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<Exception>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal(expectedException.Message, actualException.Message);
        Assert.Empty(DbContext.Incidents);
    }

    [Fact]
    public async Task Handle_WhenCurrentUserAccessorReturnsEmptyString_ShouldStillCreateIncident()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var command = new CaptureIncident.Command(1, mockFile.Object, "Test incident", DateTimeOffset.Now);

        _mockUploadFileService.Setup(x => x.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                             .ReturnsAsync("uploads/test.jpg");
        _mockCurrentUserAccessor.Setup(x => x.GetUserEmpNo()).Returns("");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        var incident = DbContext.Incidents.FirstOrDefault();
        Assert.NotNull(incident);
        Assert.Equal("", incident.CreatedBy);
    }

    [Fact]
    public async Task Validator_WhenValidCommand_ShouldPassValidation()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var command = new CaptureIncident.Command(1, mockFile.Object, "Valid description", DateTimeOffset.Now);

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
        var mockFile = new Mock<IFormFile>();
        var command = new CaptureIncident.Command(0, mockFile.Object, "Valid description", DateTimeOffset.Now);

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
        var mockFile = new Mock<IFormFile>();
        var command = new CaptureIncident.Command(-1, mockFile.Object, "Valid description", DateTimeOffset.Now);

        // Act
        var validationResult = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.Single(validationResult.Errors);
        Assert.Equal("Premise ID must be greater than 0.", validationResult.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task Validator_WhenFileIsNull_ShouldFailValidation()
    {
        // Arrange
        var command = new CaptureIncident.Command(1, null!, "Valid description", DateTimeOffset.Now);

        // Act
        var validationResult = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.Single(validationResult.Errors);
        Assert.Equal("File is required.", validationResult.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task Validator_WhenDescriptionIsEmpty_ShouldFailValidation()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var command = new CaptureIncident.Command(1, mockFile.Object, "", DateTimeOffset.Now);

        // Act
        var validationResult = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.Single(validationResult.Errors);
        Assert.Equal("Description cannot be empty.", validationResult.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task Validator_WhenDescriptionIsNull_ShouldFailValidation()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var command = new CaptureIncident.Command(1, mockFile.Object, null!, DateTimeOffset.Now);

        // Act
        var validationResult = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.Single(validationResult.Errors);
        Assert.Equal("Description cannot be empty.", validationResult.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task Validator_WhenMultipleFieldsAreInvalid_ShouldReturnMultipleErrors()
    {
        // Arrange
        var command = new CaptureIncident.Command(0, null!, "", DateTimeOffset.Now);

        // Act
        var validationResult = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.Equal(3, validationResult.Errors.Count);
        Assert.Contains(validationResult.Errors, e => e.ErrorMessage == "Premise ID must be greater than 0.");
        Assert.Contains(validationResult.Errors, e => e.ErrorMessage == "File is required.");
        Assert.Contains(validationResult.Errors, e => e.ErrorMessage == "Description cannot be empty.");
    }
}
