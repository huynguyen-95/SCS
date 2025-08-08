namespace SCS.MediaPublisher.Services;

public interface IFileWatcherService
{
    void StartWatching(string folderPath);

    void StopWatching();
}

public class FileWatcherService : IFileWatcherService
{
    private FileSystemWatcher _watcher;
    private readonly IUploadFileService _uploadFileService;

    public FileWatcherService(IUploadFileService uploadFileService)
    {
        ArgumentNullException.ThrowIfNull(uploadFileService, nameof(uploadFileService));

        _uploadFileService = uploadFileService;
    }

    public void StartWatching(string folderPath)
    {
        if (_watcher != null)
        {
            throw new InvalidOperationException("File watcher is already running.");
        }

        _watcher = new FileSystemWatcher(folderPath)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size,
            Filter = "*.ts",
            IncludeSubdirectories = true
        };

        Console.WriteLine($"Watching folder: {folderPath}");
        _watcher.Created += async (s, e) =>
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
                await _uploadFileService.UploadFileAsync(filePath, fileName);

                var m3u8Files = Directory.GetFiles(directory, "*.m3u8");
                if (m3u8Files.Length != 0)
                {
                    var m3u8FilePath = m3u8Files[0];
                    var m3u8FileName = Path.Join(cameraId, Path.GetFileName(m3u8FilePath));
                    Console.WriteLine($"Found associated m3u8 file: {m3u8FileName}");

                    // Upload the m3u8 file to S3
                    await Task.Delay(100); // Optional delay to ensure file is fully written
                    await _uploadFileService.UploadFileAsync(m3u8FilePath, m3u8FileName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading file: {ex.Message}");
            }
        };
        _watcher.EnableRaisingEvents = true;
    }

    public void StopWatching()
    {
        if (_watcher == null)
        {
            throw new InvalidOperationException("File watcher is not running.");
        }

        _watcher.EnableRaisingEvents = false;
        _watcher.Dispose();
        _watcher = null;
    }
}
