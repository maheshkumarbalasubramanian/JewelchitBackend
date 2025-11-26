using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using JewelChitApplication.Data;
using JewelChitApplication.Services;

// PostgreSQL timestamp behavior
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// ============ LOGGING (Railway captures stdout) ============
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// ============ PORT CONFIGURATION (Critical for Railway) ============
var portStr = Environment.GetEnvironmentVariable("PORT") ?? "8080";
if (!int.TryParse(portStr, out var port)) port = 8080;

// Set ASPNETCORE_URLS environment variable
Environment.SetEnvironmentVariable("ASPNETCORE_URLS", $"http://0.0.0.0:{port}");

// Configure Kestrel to listen on Any (covers both v4 and v6)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(port);
});

// ============ DATABASE CONFIGURATION ============
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

Console.WriteLine($"[DATABASE] Connection string source: {(Environment.GetEnvironmentVariable("DATABASE_URL") != null ? "Railway DATABASE_URL" : "appsettings.json")}");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        connectionString,
        npgsqlOptions => npgsqlOptions.CommandTimeout(60)
    ));

// ============ CONTROLLERS & JSON OPTIONS ============
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// ============ SERVICES ============
builder.Services.AddScoped<ICustomerService, CustomerService>();

// ============ CORS CONFIGURATION ============
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(
            "https://j-chit-frontend.vercel.app",
            "http://localhost:4200",  // Angular dev
            "http://localhost:5000",  // API HTTP
            "https://localhost:5001"  // API HTTPS
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// ============ FILE UPLOAD CONFIGURATION ============
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB
});

// ============ SWAGGER/OPENAPI ============
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Gold Mortgage API",
        Version = "v1",
        Description = "API for Gold Mortgage and Chit Fund Management System"
    });
});

// ============ BUILD APP ============
var app = builder.Build();

// ============ LIFETIME LOGGING ============
app.Lifetime.ApplicationStarted.Register(() =>
    Console.WriteLine("[LIFETIME] ApplicationStarted " + DateTime.UtcNow));
app.Lifetime.ApplicationStopping.Register(() =>
    Console.WriteLine("[LIFETIME] ApplicationStopping " + DateTime.UtcNow));
app.Lifetime.ApplicationStopped.Register(() =>
    Console.WriteLine("[LIFETIME] ApplicationStopped " + DateTime.UtcNow));

Console.WriteLine($"[STARTUP] App starting. PORT={port} (ASPNETCORE_URLS={Environment.GetEnvironmentVariable("ASPNETCORE_URLS")})");
Console.Out.Flush();

// ============ SWAGGER UI ============
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gold Mortgage API V1");
        c.RoutePrefix = "swagger";
    });
}

// ============ STATIC FILES ============
var uploadsPath = Path.Combine(app.Environment.WebRootPath ?? "./wwwroot", "uploads", "company-logos");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
    Console.WriteLine($"[STARTUP] Created uploads directory: {uploadsPath}");
}

app.UseStaticFiles();

// ============ MIDDLEWARE ============
app.UseCors("AllowAngular");
// app.UseHttpsRedirection(); // Disabled for Railway (it handles HTTPS)
app.UseAuthorization();

// ============ HEALTH CHECK ENDPOINT ============
app.MapGet("/health", () =>
    Results.Text($"ok {DateTime.UtcNow:O}"));

app.MapGet("/", () => "Hello World");

// ============ CONTROLLERS ============
app.MapControllers();

// ============ DATABASE MIGRATION & SEEDING ============
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        Console.WriteLine("[DATABASE] Attempting to connect and initialize database...");

        // Test connection
        await context.Database.OpenConnectionAsync();
        Console.WriteLine("[DATABASE] ✓ Connected successfully");

        // Ensure database is created
        await context.Database.EnsureCreatedAsync();
        Console.WriteLine("[DATABASE] ✓ Database created/verified");

        // Or use migrations instead:
        // await context.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "[DATABASE] ✗ Error during database initialization");
        Console.Error.WriteLine($"[DATABASE] Error: {ex.Message}");
        // Don't throw - app can still run without DB in some cases
    }
}

// ============ RUN APP ============
try
{
    Console.WriteLine("[STARTUP] All systems ready. Starting application...");
    Console.Out.Flush();
    await app.RunAsync();
}
catch (Exception ex)
{
    Console.Error.WriteLine("[FATAL] " + ex);
    Console.Error.Flush();
    throw;
}