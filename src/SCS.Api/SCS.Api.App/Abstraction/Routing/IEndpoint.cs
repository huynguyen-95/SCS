namespace SCS.Api.App.Abstraction.Routing;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
