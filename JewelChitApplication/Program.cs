var builder = WebApplication.CreateBuilder(args);

// console logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// bind to the PORT env var (Railway sets PORT), fallback to 8080
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(int.Parse(port)));

builder.Services.AddControllers();
var app = builder.Build();

Console.WriteLine("[STARTUP] App starting. PORT=" + Environment.GetEnvironmentVariable("PORT"));
Console.Out.Flush();

app.MapGet("/", () => "Hello World");
app.MapGet("/health", () => Results.Text($"ok {DateTime.UtcNow:O}"));
app.MapControllers();

app.Run();
