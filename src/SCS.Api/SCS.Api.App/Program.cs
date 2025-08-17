using FluentValidation;
using Scalar.AspNetCore;
using SCS.Api.App.Extensions;
using SCS.Api.App.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.ConfigureInfrastructure(builder.Configuration);
builder.Services.RegisterDatabase(builder.Configuration);
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddCors();
builder.Services.ConfigureAuthorization(builder.Configuration);
builder.Services.AddMemoryCache();
builder.Services.AddSignalR();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseCors(builder =>
{
    builder
        .SetIsOriginAllowed(hostName => true) // Allow any origin
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
});

app.UseAuthentication();
app.UseAuthorization();
app.AddAppEndpoints();

app.MapHub<AlarmSystemHub>("/hubs/alarm-system");

app.Run();
