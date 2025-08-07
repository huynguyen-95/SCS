using Microsoft.Extensions.Configuration;
using SCS.MediaPublisher.Services;

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

IConfiguration configuration = builder.Build();

Console.WriteLine("Media Publisher is running!");
var cameraHlsFolder = configuration.GetValue<string>("CameraHLSFolder") ?? "./camera-hls";
Console.WriteLine($"Watching folder: {cameraHlsFolder}");

// Set up a FileSystemWatcher to monitor the camera HLS folder
var fileUploadService = new UploadFileService(configuration);
var watcher = new FileSystemWatcher(cameraHlsFolder)
{
    NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size,
    Filter = "*.ts",
    IncludeSubdirectories = true
};

watcher.Created += async (s, e) =>
{
    var filePath = e.FullPath;
    var fileName = e.Name;
    var directory = Path.GetDirectoryName(filePath);
    var cameraId = Path.GetDirectoryName(fileName);

    Console.WriteLine($"Detected file: {fileName} in {directory}");
    try
    {
        // Upload the file to S3
        await Task.Delay(100); // Optional delay to ensure file is fully written
        await fileUploadService.UploadFileAsync(filePath, fileName);

        var m3u8Files = Directory.GetFiles(directory, "*.m3u8");
        if (m3u8Files.Length != 0)
        {
            var m3u8FilePath = m3u8Files[0];
            var m3u8FileName = Path.Join(cameraId, Path.GetFileName(m3u8FilePath));
            Console.WriteLine($"Found associated m3u8 file: {m3u8FileName}");

            // Upload the m3u8 file to S3
            await Task.Delay(100); // Optional delay to ensure file is fully written
            await fileUploadService.UploadFileAsync(m3u8FilePath, m3u8FileName);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error uploading file: {ex.Message}");
    }
};

watcher.EnableRaisingEvents = true;

// Keep the application running until Ctrl+C is pressed
var exitEvent = new TaskCompletionSource<bool>();

Console.CancelKeyPress += (sender, eventArgs) =>
{
    eventArgs.Cancel = true;
    exitEvent.SetResult(true);
};

Console.WriteLine("Press Ctrl+C to exit the application...");
await exitEvent.Task;
