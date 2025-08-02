using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateSlimBuilder(args);

var app = builder.Build();

app.UseStaticFiles();

app.Run();
