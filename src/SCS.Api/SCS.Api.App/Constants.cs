using System.Text.Json;

namespace SCS.Api.App;

public static class Constants
{
    public static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
}
