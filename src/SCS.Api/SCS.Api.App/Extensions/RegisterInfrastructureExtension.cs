using System.Reflection;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SimpleEmail;
using Amazon.SQS;
using SCS.Api.App.Abstraction.Messaging;
using SCS.Api.App.Consumers;
using SCS.Api.App.Helpers;
using SCS.Api.App.Services;
using SCS.Api.App.Settings;

namespace SCS.Api.App.Extensions;

public static class RegisterInfrastructureExtension
{
    public static void ConfigureInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        RegisterRequestHandlers(services);
        RegisterAWSComponents(services, configuration);
        RegisterJWTComponents(services, configuration);
        RegisterApplicationServices(services);
    }

    private static void RegisterApplicationServices(IServiceCollection services)
    {
        // SQS Consumers
        services.AddHostedService<AlarmSystemAlertConsumer>();

        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
        services.AddScoped<IUploadFileService, UploadFileService>();
    }

    private static void RegisterJWTComponents(IServiceCollection services, IConfiguration configuration)
    {
        // Configure JWT
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
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

    private static void RegisterAWSComponents(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AwsOptions>(configuration.GetSection(AwsOptions.ConfigurationSection));
        var awsSettings = configuration.GetSection(AwsOptions.ConfigurationSection).Get<AwsOptions>();

        services.AddOptions<AwsOptions>()
            .Bind(configuration.GetSection(AwsOptions.ConfigurationSection))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IAmazonSimpleEmailService>(_ =>
        {
            var region = RegionEndpoint.GetBySystemName(awsSettings.Region);
            var creds = new BasicAWSCredentials(awsSettings.AccessKey, awsSettings.SecretKey);

            return new AmazonSimpleEmailServiceClient(creds, region);
        });

        services.AddScoped<IAmazonS3>(_ =>
        {
            return new AmazonS3Client(awsSettings.AccessKey, awsSettings.SecretKey, new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(awsSettings.Region)
            });
        });

        services.AddScoped<IAmazonSQS>(_ =>
        {
            return new AmazonSQSClient(
                awsSettings.AccessKey,
                awsSettings.SecretKey,
                RegionEndpoint.GetBySystemName(awsSettings.Region));
        });
    }
}
