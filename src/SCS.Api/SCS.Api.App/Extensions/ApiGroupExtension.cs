namespace SCS.Api.App.Extensions;

public static class ApiGroupExtension
{
    public static RouteGroupBuilder AuthenticationGroup(this IEndpointRouteBuilder app) =>
        app.MapGroup("api/authentication")
            .WithTags("Authentication")
            .WithDisplayName("Authentication Endpoints");

    public static RouteGroupBuilder PremiseGroup(this IEndpointRouteBuilder app) =>
        app.MapGroup("api/premise")
            .RequireAuthorization()
            .WithTags("Premise")
            .WithDisplayName("Premise Endpoints");

    public static RouteGroupBuilder AlarmSystemGroup(this IEndpointRouteBuilder app) =>
        app.MapGroup("api/alarm-system")
            .RequireAuthorization()
            .WithTags("Alarm System")
            .WithDisplayName("Alarm System Endpoints");
}
