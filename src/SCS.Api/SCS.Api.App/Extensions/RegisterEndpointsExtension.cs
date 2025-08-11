using SCS.Api.App.Abstraction.Routing;

namespace SCS.Api.App.Extensions;

public static class RegisterEndpointsExtension
{
    public static void AddAppEndpoints(this WebApplication app)
    {
        var endpointTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IEndpoint).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in endpointTypes)
        {
            if (Activator.CreateInstance(type) is IEndpoint endpoint)
            {
                endpoint.MapEndpoint(app);
            }
        }
    }
}
