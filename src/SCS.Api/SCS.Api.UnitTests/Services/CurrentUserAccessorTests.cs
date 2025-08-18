using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SCS.Api.App;
using SCS.Api.App.Services;

namespace SCS.Api.UnitTests.Services;

public class CurrentUserAccessorTests
{
    [Fact]
    public void Constructor_WhenHttpContextAccessorIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new CurrentUserAccessor(null!));
        Assert.Equal("httpContextAccessor", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenHttpContextAccessorIsValid_ShouldCreateInstance()
    {
        // Arrange
        var httpContextAccessor = new HttpContextAccessor();

        // Act
        var currentUserAccessor = new CurrentUserAccessor(httpContextAccessor);

        // Assert
        Assert.NotNull(currentUserAccessor);
        Assert.IsAssignableFrom<ICurrentUserAccessor>(currentUserAccessor);
    }

    [Fact]
    public void GetUserEmpNo_WhenHttpContextIsNull_ShouldReturnNull()
    {
        // Arrange
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = null
        };
        var currentUserAccessor = new CurrentUserAccessor(httpContextAccessor);

        // Act
        var result = currentUserAccessor.GetUserEmpNo();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetUserEmpNo_WhenUserIsNull_ShouldReturnNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.User = null!;

        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = httpContext
        };
        var currentUserAccessor = new CurrentUserAccessor(httpContextAccessor);

        // Act
        var result = currentUserAccessor.GetUserEmpNo();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetUserEmpNo_WhenEmpNoClaimExists_ShouldReturnClaimValue()
    {
        // Arrange
        var empNo = "EMP12345";
        var claims = new[]
        {
            new Claim(Constants.AppClaims.EmpNo, empNo),
            new Claim(Constants.AppClaims.IsAdmin, "true"),
            new Claim("other-claim", "other-value")
        };

        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext();
        httpContext.User = principal;

        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = httpContext
        };
        var currentUserAccessor = new CurrentUserAccessor(httpContextAccessor);

        // Act
        var result = currentUserAccessor.GetUserEmpNo();

        // Assert
        Assert.Equal(empNo, result);
    }

    [Fact]
    public void GetUserEmpNo_WhenEmpNoClaimDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(Constants.AppClaims.IsAdmin, "true"),
            new Claim("other-claim", "other-value")
        };

        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext();
        httpContext.User = principal;

        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = httpContext
        };
        var currentUserAccessor = new CurrentUserAccessor(httpContextAccessor);

        // Act
        var result = currentUserAccessor.GetUserEmpNo();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetUserEmpNo_WhenEmpNoClaimHasEmptyValue_ShouldReturnEmptyString()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(Constants.AppClaims.EmpNo, ""),
            new Claim(Constants.AppClaims.IsAdmin, "false")
        };

        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext();
        httpContext.User = principal;

        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = httpContext
        };
        var currentUserAccessor = new CurrentUserAccessor(httpContextAccessor);

        // Act
        var result = currentUserAccessor.GetUserEmpNo();

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void GetUserEmpNo_WhenEmpNoClaimHasWhitespaceValue_ShouldReturnWhitespace()
    {
        // Arrange
        var whiteSpace = "   ";
        var claims = new[]
        {
            new Claim(Constants.AppClaims.EmpNo, whiteSpace)
        };

        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext();
        httpContext.User = principal;

        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = httpContext
        };
        var currentUserAccessor = new CurrentUserAccessor(httpContextAccessor);

        // Act
        var result = currentUserAccessor.GetUserEmpNo();

        // Assert
        Assert.Equal(whiteSpace, result);
    }
}
