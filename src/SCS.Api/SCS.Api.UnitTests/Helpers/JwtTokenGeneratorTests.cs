using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using SCS.Api.App;
using SCS.Api.App.Helpers;
using SCS.Api.App.Settings;
using SCS.Api.Domain;

namespace SCS.Api.UnitTests.Helpers;

public class JwtTokenGeneratorTests
{
    private readonly Mock<IOptions<JwtSettings>> _mockOptions;
    private readonly JwtSettings _jwtSettings;
    private readonly JwtTokenGenerator _tokenGenerator;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public JwtTokenGeneratorTests()
    {
        _mockOptions = new Mock<IOptions<JwtSettings>>();
        _jwtSettings = new JwtSettings
        {
            Secret = "ThisIsATestSecretKeyThatMustBeAtLeast256BitsLong12345",
            ExpiryMinutes = 60,
            Issuer = "TestIssuer",
            Audience = "TestAudience"
        };

        _mockOptions.Setup(x => x.Value).Returns(_jwtSettings);
        _tokenGenerator = new JwtTokenGenerator(_mockOptions.Object);
        _tokenHandler = new JwtSecurityTokenHandler();
    }

    [Fact]
    public void Constructor_WhenJwtSettingsIsNull_ShouldThrowNullReferenceException()
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<NullReferenceException>(() =>
            new JwtTokenGenerator(null!));
    }

    [Fact]
    public void GenerateToken_WhenUserIsNull_ShouldThrowNullReferenceException()
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<NullReferenceException>(() =>
            _tokenGenerator.GenerateToken(null!));
    }

    [Fact]
    public void GenerateToken_WhenValidAdminUser_ShouldReturnValidJwtToken()
    {
        // Arrange
        var user = new User("EMP001", "admin", true);

        // Act
        var token = _tokenGenerator.GenerateToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        var jwtToken = _tokenHandler.ReadJwtToken(token);
        Assert.NotNull(jwtToken);
        Assert.Equal(_jwtSettings.Issuer, jwtToken.Issuer);
        Assert.Equal(_jwtSettings.Audience, jwtToken.Audiences.First());
    }

    [Fact]
    public void GenerateToken_WhenValidRegularUser_ShouldReturnValidJwtToken()
    {
        // Arrange
        var user = new User("EMP002", "regular_user", false);

        // Act
        var token = _tokenGenerator.GenerateToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        var jwtToken = _tokenHandler.ReadJwtToken(token);
        Assert.NotNull(jwtToken);
        Assert.Equal(_jwtSettings.Issuer, jwtToken.Issuer);
        Assert.Equal(_jwtSettings.Audience, jwtToken.Audiences.First());
    }

    [Fact]
    public void GenerateToken_WhenValidAdminUser_ShouldContainCorrectClaims()
    {
        // Arrange
        var user = new User("EMP001", "admin_user", true);

        // Act
        var token = _tokenGenerator.GenerateToken(user);

        // Assert
        var jwtToken = _tokenHandler.ReadJwtToken(token);
        var claims = jwtToken.Claims.ToList();

        Assert.Contains(claims, c => c.Type == Constants.AppClaims.EmpNo && c.Value == "EMP001");
        Assert.Contains(claims, c => c.Type == JwtRegisteredClaimNames.Name && c.Value == "admin_user");
        Assert.Contains(claims, c => c.Type == Constants.AppClaims.Role && c.Value == "Admin");
        Assert.Contains(claims, c => c.Type == JwtRegisteredClaimNames.Jti && !string.IsNullOrEmpty(c.Value));
    }

    [Fact]
    public void GenerateToken_WhenValidRegularUser_ShouldContainCorrectClaims()
    {
        // Arrange
        var user = new User("EMP002", "regular_user", false);

        // Act
        var token = _tokenGenerator.GenerateToken(user);

        // Assert
        var jwtToken = _tokenHandler.ReadJwtToken(token);
        var claims = jwtToken.Claims.ToList();

        Assert.Contains(claims, c => c.Type == Constants.AppClaims.EmpNo && c.Value == "EMP002");
        Assert.Contains(claims, c => c.Type == JwtRegisteredClaimNames.Name && c.Value == "regular_user");
        Assert.Contains(claims, c => c.Type == Constants.AppClaims.Role && c.Value == "SCS-User");
        Assert.Contains(claims, c => c.Type == JwtRegisteredClaimNames.Jti && !string.IsNullOrEmpty(c.Value));
    }

    [Fact]
    public void GenerateToken_WhenValidUser_ShouldHaveExpirySet()
    {
        // Arrange
        var user = new User("EMP001", "test_user", false);

        // Act
        var token = _tokenGenerator.GenerateToken(user);

        // Assert
        var jwtToken = _tokenHandler.ReadJwtToken(token);

        // Token should have an expiry date set (not default DateTime)
        Assert.NotEqual(default(DateTime), jwtToken.ValidTo);

        // Token should have a ValidFrom date before ValidTo
        Assert.True(jwtToken.ValidFrom < jwtToken.ValidTo);
    }

    [Fact]
    public void GenerateToken_WhenValidUser_ShouldBeVerifiableWithSecret()
    {
        // Arrange
        var user = new User("EMP001", "test_user", true);

        // Act
        var token = _tokenGenerator.GenerateToken(user);

        // Assert
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
            ClockSkew = TimeSpan.Zero
        };

        var principal = _tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);

        Assert.NotNull(principal);
        Assert.NotNull(validatedToken);
        Assert.IsType<JwtSecurityToken>(validatedToken);
    }

    [Fact]
    public void GenerateToken_WhenSpecialCharactersInUserData_ShouldGenerateValidToken()
    {
        // Arrange
        var user = new User("EMP-特殊字符", "用户名_@#$%", false);

        // Act
        var token = _tokenGenerator.GenerateToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        var jwtToken = _tokenHandler.ReadJwtToken(token);
        var claims = jwtToken.Claims.ToList();

        Assert.Contains(claims, c => c.Type == Constants.AppClaims.EmpNo && c.Value == "EMP-特殊字符");
        Assert.Contains(claims, c => c.Type == JwtRegisteredClaimNames.Name && c.Value == "用户名_@#$%");
    }

    [Fact]
    public void GenerateToken_WhenCalledMultipleTimes_ShouldGenerateUniqueTokens()
    {
        // Arrange
        var user = new User("EMP001", "test_user", false);

        // Act
        var token1 = _tokenGenerator.GenerateToken(user);
        var token2 = _tokenGenerator.GenerateToken(user);

        // Assert
        Assert.NotEqual(token1, token2);

        var jwtToken1 = _tokenHandler.ReadJwtToken(token1);
        var jwtToken2 = _tokenHandler.ReadJwtToken(token2);

        var jti1 = jwtToken1.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        var jti2 = jwtToken2.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

        Assert.NotEqual(jti1, jti2);
    }

    [Fact]
    public void GenerateToken_WhenDifferentExpirySettings_ShouldReflectInToken()
    {
        // Arrange
        var shortExpirySettings = new JwtSettings
        {
            Secret = "ThisIsATestSecretKeyThatMustBeAtLeast256BitsLong12345",
            ExpiryMinutes = 15,
            Issuer = "TestIssuer",
            Audience = "TestAudience"
        };

        var mockShortOptions = new Mock<IOptions<JwtSettings>>();
        mockShortOptions.Setup(x => x.Value).Returns(shortExpirySettings);
        var shortExpiryGenerator = new JwtTokenGenerator(mockShortOptions.Object);

        var user = new User("EMP001", "test_user", false);

        // Act
        var normalToken = _tokenGenerator.GenerateToken(user);
        var shortToken = shortExpiryGenerator.GenerateToken(user);

        // Assert
        var normalJwtToken = _tokenHandler.ReadJwtToken(normalToken);
        var shortJwtToken = _tokenHandler.ReadJwtToken(shortToken);

        Assert.True(normalJwtToken.ValidTo > shortJwtToken.ValidTo);
    }

    [Fact]
    public void GenerateToken_WhenEmptyEmpNo_ShouldGenerateTokenWithEmptyClaim()
    {
        // Arrange
        var user = new User("", "test_user", false);

        // Act
        var token = _tokenGenerator.GenerateToken(user);

        // Assert
        var jwtToken = _tokenHandler.ReadJwtToken(token);
        var claims = jwtToken.Claims.ToList();

        Assert.Contains(claims, c => c.Type == Constants.AppClaims.EmpNo && c.Value == "");
    }

    [Fact]
    public void GenerateToken_WhenEmptyUsername_ShouldGenerateTokenWithEmptyClaim()
    {
        // Arrange
        var user = new User("EMP001", "", true);

        // Act
        var token = _tokenGenerator.GenerateToken(user);

        // Assert
        var jwtToken = _tokenHandler.ReadJwtToken(token);
        var claims = jwtToken.Claims.ToList();

        Assert.Contains(claims, c => c.Type == JwtRegisteredClaimNames.Name && c.Value == "");
    }

    [Fact]
    public void JwtSettings_ShouldHaveCorrectSectionName()
    {
        // Arrange & Act
        var sectionName = JwtSettings.SectionName;

        // Assert
        Assert.Equal("JwtSettings", sectionName);
    }
}
