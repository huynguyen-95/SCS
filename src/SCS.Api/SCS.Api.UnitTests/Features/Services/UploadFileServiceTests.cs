using Amazon.S3;
using Amazon.S3.Model;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using SCS.Api.App.Services;
using SCS.Api.App.Settings;
using System.Net;

namespace SCS.Api.UnitTests.Features.Services;

public class UploadFileServiceTests
{
    private readonly Mock<IAmazonS3> _mockS3Client;
    private readonly Mock<IOptions<AwsOptions>> _mockOptions;
    private readonly AwsOptions _awsOptions;
    private readonly UploadFileService _uploadFileService;

    public UploadFileServiceTests()
    {
        _mockS3Client = new Mock<IAmazonS3>();
        _mockOptions = new Mock<IOptions<AwsOptions>>();

        _awsOptions = new AwsOptions
        {
            BucketName = "test-bucket",
            Region = "us-east-1",
            AccessKey = "test-access-key",
            SecretKey = "test-secret-key",
            QueueUrl = "test-queue-url"
        };

        _mockOptions.Setup(x => x.Value).Returns(_awsOptions);
        _uploadFileService = new UploadFileService(_mockOptions.Object, _mockS3Client.Object);
    }

    [Fact]
    public void Constructor_WhenOptionsIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new UploadFileService(null!, _mockS3Client.Object));
        Assert.Equal("options", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenS3ClientIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new UploadFileService(_mockOptions.Object, null!));
        Assert.Equal("s3Client", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var service = new UploadFileService(_mockOptions.Object, _mockS3Client.Object);

        // Assert
        Assert.NotNull(service);
        Assert.IsAssignableFrom<IUploadFileService>(service);
    }

    [Fact]
    public async Task UploadFileAsync_WhenFileIsNull_ShouldThrowArgumentException()
    {
        // Arrange, Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _uploadFileService.UploadFileAsync(null!, CancellationToken.None));
        Assert.Equal("File is required.", exception.Message);
    }

    [Fact]
    public async Task UploadFileAsync_WhenFileIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(0);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _uploadFileService.UploadFileAsync(mockFile.Object, CancellationToken.None));
        Assert.Equal("File is required.", exception.Message);
    }

    [Fact]
    public async Task UploadFileAsync_WhenSuccessful_ShouldCallPutObjectAsync()
    {
        // Arrange
        var mockFile = CreateMockFile("test.jpg", "image/jpeg", 1024);

        _mockS3Client.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new PutObjectResponse { HttpStatusCode = HttpStatusCode.OK });

        // Act
        await _uploadFileService.UploadFileAsync(mockFile.Object, CancellationToken.None);

        // Assert
        _mockS3Client.Verify(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UploadFileAsync_WhenSuccessful_ShouldReturnCorrectUrl()
    {
        // Arrange
        var mockFile = CreateMockFile("test.jpg", "image/jpeg", 1024);

        _mockS3Client.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new PutObjectResponse { HttpStatusCode = HttpStatusCode.OK });

        // Act
        var result = await _uploadFileService.UploadFileAsync(mockFile.Object, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.StartsWith($"https://{_awsOptions.BucketName}.s3.{_awsOptions.Region}.amazonaws.com/", result.Value);
        Assert.EndsWith(".jpg", result.Value);
    }

    [Fact]
    public async Task UploadFileAsync_WhenSuccessful_ShouldPassCorrectPutObjectRequest()
    {
        // Arrange
        var mockFile = CreateMockFile("test.png", "image/png", 2048);
        PutObjectRequest? capturedRequest = null;

        _mockS3Client.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
                     .Callback<PutObjectRequest, CancellationToken>((req, token) => capturedRequest = req)
                     .ReturnsAsync(new PutObjectResponse { HttpStatusCode = HttpStatusCode.OK });

        // Act
        await _uploadFileService.UploadFileAsync(mockFile.Object, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal(_awsOptions.BucketName, capturedRequest.BucketName);
        Assert.EndsWith(".png", capturedRequest.Key);
        Assert.Equal("image/png", capturedRequest.ContentType);
        Assert.NotNull(capturedRequest.InputStream);
    }

    [Fact]
    public async Task UploadFileAsync_WhenS3ReturnsNonOkStatus_ShouldReturnError()
    {
        // Arrange
        var mockFile = CreateMockFile("test.pdf", "application/pdf", 1024);

        _mockS3Client.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new PutObjectResponse { HttpStatusCode = HttpStatusCode.InternalServerError });

        // Act
        var result = await _uploadFileService.UploadFileAsync(mockFile.Object, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Single(result.Errors);

        var error = result.FirstError;
        Assert.Equal(ErrorType.Failure, error.Type);
        Assert.Equal("UploadFileService.UploadFailed", error.Code);
        Assert.Equal("Failed to upload file to S3.", error.Description);
    }

    [Fact]
    public async Task UploadFileAsync_WhenS3ThrowsException_ShouldPropagateException()
    {
        // Arrange
        var mockFile = CreateMockFile("test.txt", "text/plain", 512);
        var expectedException = new Exception("S3 Error");

        _mockS3Client.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(expectedException);

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<Exception>(() =>
            _uploadFileService.UploadFileAsync(mockFile.Object, CancellationToken.None));

        Assert.Equal(expectedException.Message, actualException.Message);
        _mockS3Client.Verify(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UploadFileAsync_WithDifferentFileTypes_ShouldCallPutObjectAsync()
    {
        // Arrange
        var files = new[]
        {
            CreateMockFile("image.jpg", "image/jpeg", 1024),
            CreateMockFile("document.pdf", "application/pdf", 2048),
            CreateMockFile("video.mp4", "video/mp4", 4096)
        };

        _mockS3Client.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new PutObjectResponse { HttpStatusCode = HttpStatusCode.OK });

        // Act
        foreach (var file in files)
        {
            await _uploadFileService.UploadFileAsync(file.Object, CancellationToken.None);
        }

        // Assert
        _mockS3Client.Verify(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task UploadFileAsync_WithCancellationToken_ShouldPassTokenToS3Client()
    {
        // Arrange
        var mockFile = CreateMockFile("test.doc", "application/msword", 1024);
        var cancellationToken = new CancellationTokenSource().Token;

        _mockS3Client.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), cancellationToken))
                     .ReturnsAsync(new PutObjectResponse { HttpStatusCode = HttpStatusCode.OK });

        // Act
        await _uploadFileService.UploadFileAsync(mockFile.Object, cancellationToken);

        // Assert
        _mockS3Client.Verify(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task UploadFileAsync_ShouldGenerateUniqueKeys()
    {
        // Arrange
        var mockFile = CreateMockFile("test.jpg", "image/jpeg", 1024);
        var capturedKeys = new List<string>();

        _mockS3Client.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
                     .Callback<PutObjectRequest, CancellationToken>((req, token) => capturedKeys.Add(req.Key))
                     .ReturnsAsync(new PutObjectResponse { HttpStatusCode = HttpStatusCode.OK });

        // Act
        await _uploadFileService.UploadFileAsync(mockFile.Object, CancellationToken.None);
        await _uploadFileService.UploadFileAsync(mockFile.Object, CancellationToken.None);

        // Assert
        Assert.Equal(2, capturedKeys.Count);
        Assert.NotEqual(capturedKeys[0], capturedKeys[1]);
        Assert.All(capturedKeys, key => Assert.EndsWith(".jpg", key));
    }

    private static Mock<IFormFile> CreateMockFile(string fileName, string contentType, long length)
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.ContentType).Returns(contentType);
        mockFile.Setup(f => f.Length).Returns(length);
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[length]));
        return mockFile;
    }
}
