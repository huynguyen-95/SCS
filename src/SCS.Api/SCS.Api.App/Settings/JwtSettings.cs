namespace SCS.Api.App.Settings;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";
    public required string Secret { get; init; } = null!;
    public required int ExpiryMinutes { get; init; }
    public required string Issuer { get; init; } = null!;
    public required string Audience { get; init; } = null!;
}
