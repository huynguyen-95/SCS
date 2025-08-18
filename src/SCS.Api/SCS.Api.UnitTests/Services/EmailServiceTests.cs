using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Configuration;
using Moq;
using SCS.Api.App.Services;

namespace SCS.Api.UnitTests.Services;

public class EmailServiceTests
{
    private readonly Mock<IAmazonSimpleEmailService> _mockAwsSes;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly EmailService _emailService;

    public EmailServiceTests()
    {
        _mockAwsSes = new Mock<IAmazonSimpleEmailService>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(x => x["Email:From"]).Returns("test@example.com");

        _emailService = new EmailService(_mockAwsSes.Object, _mockConfiguration.Object);
    }

    [Fact]
    public void Constructor_WhenAwsSesIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new EmailService(null!, _mockConfiguration.Object));
        Assert.Equal("awsSES", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var service = new EmailService(_mockAwsSes.Object, _mockConfiguration.Object);

        // Assert
        Assert.NotNull(service);
        Assert.IsAssignableFrom<IEmailService>(service);
    }

    [Fact]
    public async Task SendEmailAsync_WhenCalled_ShouldCallAwsSendEmailAsync()
    {
        // Arrange
        var to = "recipient@example.com";
        var subject = "Test Subject";
        var message = "Test Message";

        _mockAwsSes.Setup(x => x.SendEmailAsync(It.IsAny<SendEmailRequest>(), default))
                   .Returns(Task.FromResult(new SendEmailResponse()));

        // Act
        await _emailService.SendEmailAsync(to, subject, message);

        // Assert
        _mockAwsSes.Verify(x => x.SendEmailAsync(It.IsAny<SendEmailRequest>(), default), Times.Once);
    }
}
