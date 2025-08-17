using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SCS.Api.Domain;

namespace SCS.Api.App.Helpers;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}

public class JwtSettings
{
    public const string SectionName = "JwtSettings";
    public required string Secret { get; init; } = null!;
    public required int ExpiryMinutes { get; init; }
    public required string Issuer { get; init; } = null!;
    public required string Audience { get; init; } = null!;
}


public class JwtTokenGenerator(IOptions<JwtSettings> jwtSettings) : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    public string GenerateToken(User user)
    {
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(Constants.AppClaims.EmpNo, user.EmpNo),
            new Claim(JwtRegisteredClaimNames.Name, user.Username),
            new Claim(Constants.AppClaims.Role, user.IsAdmin ? "Admin" : "SCS-User"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var securityToken = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            expires: DateTime.Now.AddMinutes(_jwtSettings.ExpiryMinutes),
            claims: claims,
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(securityToken);
    }
}
