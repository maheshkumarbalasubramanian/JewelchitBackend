using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Force all logging to console
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// IMPORTANT: bind to Railway-provided PORT and 0.0.0.0
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

builder.Services.AddControllers();

var app = builder.Build();

// helpful startup logs
Console.WriteLine("[STARTUP] App starting. PORT=" + Environment.GetEnvironmentVariable("PORT"));
Console.Out.Flush();

app.MapGet("/", () => "Hello World");
app.MapGet("/health", () => Results.Json(new { status = "healthy", utc = DateTime.UtcNow }));
app.MapControllers();

try
{
    Console.WriteLine("[STARTUP] Calling app.Run()");
    Console.Out.Flush();
    app.Run();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"[FATAL ERROR] {ex}");
    Console.Error.Flush();
    throw;
}
