using System.Reflection;
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleEmail;
using SCS.Api.App.Abstraction.Messaging;
using SCS.Api.App.Consumers;
using SCS.Api.App.Features;
using SCS.Api.App.Helpers;
using SCS.Api.App.Settings;

namespace SCS.Api.App.Extensions;

public static class RegisterInfrastructureExtension
{
    public static void ConfigureInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AwsOptions>(configuration.GetSection(AwsOptions.ConfigurationSection));

        services.AddOptions<AwsOptions>()
            .Bind(configuration.GetSection(AwsOptions.ConfigurationSection))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Configure JWT
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        RegisterRequestHandlers(services);

        // SQS Consumers
        services.AddHostedService<AlarmSystemAlertConsumer>();

        services.AddSingleton<IAmazonSimpleEmailService>(_ =>
        {
            var awsSettings = configuration.GetSection(AwsOptions.ConfigurationSection).Get<AwsOptions>();
            var region = RegionEndpoint.GetBySystemName(awsSettings.Region);
            var creds = new BasicAWSCredentials(awsSettings.AccessKey, awsSettings.SecretKey);

            return new AmazonSimpleEmailServiceClient(creds, region);
        });
        services.AddScoped<IEmailService, EmailService>();
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
