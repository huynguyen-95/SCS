using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateSlimBuilder(args);
var configuration = builder.Configuration;
var cameraHLSFolder = configuration.GetValue<string>("CameraHLSFolder");

var app = builder.Build();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(cameraHLSFolder),
    RequestPath = "/hls",
});

app.Run();
