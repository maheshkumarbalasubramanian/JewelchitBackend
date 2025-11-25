var builder = WebApplication.CreateBuilder(args);

// Force all logging to console
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllers();

var app = builder.Build();

app.MapGet("/", () => "Hello World");
app.MapControllers();

try
{
    Console.WriteLine("[INFO] About to run app...");
    System.Console.Out.Flush();
    System.Console.Error.Flush();
    app.Run();
}
catch (Exception ex)
{
    System.Console.Error.WriteLine($"[FATAL ERROR] {ex.Message}");
    System.Console.Error.WriteLine($"[FATAL STACK] {ex.StackTrace}");
    System.Console.Error.Flush();
}