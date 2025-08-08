using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SCS.MediaPublisher.Options;
using SCS.MediaPublisher.Services;

namespace SCS.MediaPublisher.Extensions;

public static class RegisterServicesExtension
{
    public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure AWS S3 options from the configuration
        services.Configure<AwsS3Options>(configuration.GetSection(AwsS3Options.ConfigurationSection));

        // Register the UploadFileService as a singleton
        services.AddSingleton<IUploadFileService, UploadFileService>();
        services.AddSingleton<IFileWatcherService, FileWatcherService>();

        return services;
    }
}
