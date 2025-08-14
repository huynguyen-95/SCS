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
}
