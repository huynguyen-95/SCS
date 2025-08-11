using System.Reflection;
using SCS.Api.App.Abstraction.Messaging;

namespace SCS.Api.App.Extensions;

public static class RegisterInfrastructureExtension
{
    public static void ConfigureInfrastructure(this IServiceCollection services)
    {
        RegisterRequestHandlers(services);
    }

    private static void RegisterRequestHandlers(IServiceCollection services)
    {
        var handlerTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Select(t => new
            {
                Type = t,
                Interfaces = t.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                    .ToList()
            })
            .Where(t => t.Interfaces.Any())
            .ToList();

        foreach (var handler in handlerTypes)
        {
            foreach (var handlerInterface in handler.Interfaces)
            {
                services.AddScoped(handlerInterface, handler.Type);
            }
        }
    }
}
