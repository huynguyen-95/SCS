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

    public static RouteGroupBuilder SecurityGuardGroup(this IEndpointRouteBuilder app) =>
        app.MapGroup("api/security-guard")
            .RequireAuthorization()
            .WithTags("Security Guard")
            .WithDisplayName("Security Guard Endpoints");

    public static RouteGroupBuilder UserGroup(this IEndpointRouteBuilder app) =>
        app.MapGroup("api/user")
            .RequireAuthorization()
            .WithTags("User")
            .WithDisplayName("User Endpoints");
}
