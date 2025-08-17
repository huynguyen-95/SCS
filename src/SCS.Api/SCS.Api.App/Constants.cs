using System.Text.Json;

namespace SCS.Api.App;

public static class Constants
{
    public static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static class AppClaims
    {
        public const string EmpNo = "emp-no";
        public const string IsAdmin = "is-admin";
        public const string Role = "role";
    }
}
