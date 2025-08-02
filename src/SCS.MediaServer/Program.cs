using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateSlimBuilder(args);
var configuration = builder.Configuration;
var cameraHLSFolder = configuration.GetValue<string>("CameraHLSFolder");

builder.Services.AddCors();

// Add logging
builder.Logging.AddConsole();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation($"Using camera HLS folder: {cameraHLSFolder}");

var provider = new FileExtensionContentTypeProvider();
// Add new mappings
provider.Mappings[".m3u8"] = "application/x-mpegURL";
provider.Mappings[".ts"] = "video/MP2T";

app.UseCors(policy =>
{
    policy
        .AllowAnyMethod()
        .AllowCredentials()
        .SetIsOriginAllowed((host) => true)
        .AllowAnyHeader();
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(cameraHLSFolder),
    RequestPath = "/hls",
    ServeUnknownFileTypes = true, // This is important for m3u8 files
    ContentTypeProvider = provider,
});

app.MapGet("/", () => "Smart City Surveillance Media Server");

app.Run();
