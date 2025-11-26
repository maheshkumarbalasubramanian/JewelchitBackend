using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Logging to console (Railway captures stdout)
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Read Railway PORT (fallback to 8080)
var portStr = Environment.GetEnvironmentVariable("PORT") ?? "8080";
if (!int.TryParse(portStr, out var port)) port = 8080;

// Also set ASPNETCORE_URLS env var so any other framework parts follow it
Environment.SetEnvironmentVariable("ASPNETCORE_URLS", $"http://0.0.0.0:{port}");

// Configure Kestrel to listen on Any (covers both v4 and v6 where supported)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(port);
});

builder.Services.AddControllers();

var app = builder.Build();

Console.WriteLine($"[STARTUP] App starting. PORT={port} (ASPNETCORE_URLS={Environment.GetEnvironmentVariable("ASPNETCORE_URLS")})");
Console.Out.Flush();

app.MapGet("/", () => "Hello World");
app.MapGet("/health", () => Results.Text($"ok {DateTime.UtcNow:O}"));
app.MapControllers();

try
{
    app.Run();
}
catch (Exception ex)
{
    Console.Error.WriteLine("[FATAL] " + ex);
    Console.Error.Flush();
    throw;
}
