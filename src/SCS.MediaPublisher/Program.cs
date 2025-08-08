using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SCS.MediaPublisher.Extensions;
using SCS.MediaPublisher.Services;

var services = new ServiceCollection();

// Configure configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

services.RegisterServices(configuration);

var serviceProvider = services.BuildServiceProvider();

Console.WriteLine("Media Publisher is running!");
var cameraHlsFolder = configuration.GetValue<string>("CameraHLSFolder") ?? "./camera-hls";
Console.WriteLine($"Watching folder: {cameraHlsFolder}");

// Start the file watcher service
var fileWatcherService =
    serviceProvider.GetRequiredService<IFileWatcherService>() ??
    throw new InvalidOperationException("FileWatcherService is not registered.");
fileWatcherService.StartWatching(cameraHlsFolder);

// Keep the application running until Ctrl+C is pressed
var exitEvent = new TaskCompletionSource<bool>();

Console.CancelKeyPress += (sender, eventArgs) =>
{
    eventArgs.Cancel = true;
    exitEvent.SetResult(true);
};

Console.WriteLine("Press Ctrl+C to exit the application...");
await exitEvent.Task;
