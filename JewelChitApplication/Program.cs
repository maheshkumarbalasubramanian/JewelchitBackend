using Microsoft.EntityFrameworkCore;
using JewelChitApplication.Data;
using JewelChitApplication.Services;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
// ?? builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("DATABASE_URL environment variable is not set!");
}

// Convert postgresql:// URL to Npgsql connection string if needed
if (connectionString.StartsWith("postgresql://"))
{
    var uri = new Uri(connectionString);
    var db = uri.LocalPath.TrimStart('/');

    connectionString = $"Host={uri.Host};Port={uri.Port};Database={db};Username={uri.UserInfo.Split(':')[0]};Password={uri.UserInfo.Split(':')[1]};";
    Console.WriteLine($"[INFO] Converted connection string");
}

Console.WriteLine($"[INFO] Connection string received: OK");


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

try
{
    Console.WriteLine("[INFO] Adding DbContext...");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseNpgsql(connectionString);
    });
    Console.WriteLine("[INFO] DbContext added successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"[ERROR] Failed to add DbContext: {ex.GetType().Name}");
    Console.WriteLine($"[ERROR] Message: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"[ERROR] Inner: {ex.InnerException.Message}");
    }
    throw;
}

builder.Services.AddScoped<ICustomerService, CustomerService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(
            "https://j-chit-frontend.vercel.app",
            "http://localhost:4200",
            "http://localhost:5000",
            "https://localhost:5001"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Jewel Chit API",
        Version = "v1",
        Description = "API for Jewel Chit Fund Management System"
    });
});

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Jewel Chit API V1");
        c.RoutePrefix = "swagger";
    });
}

var uploadsPath = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "uploads", "company-logos");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles();
app.UseCors("AllowAngular");
app.UseAuthorization();
app.MapControllers();

// Database Migration
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        Console.WriteLine("[INFO] Getting DbContext for migration...");
        var context = services.GetRequiredService<ApplicationDbContext>();

        Console.WriteLine("[INFO] Starting database migration...");
        context.Database.EnsureCreated();
        Console.WriteLine("[INFO] Database migration completed!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] Database migration failed!");
        Console.WriteLine($"[ERROR] Exception Type: {ex.GetType().Name}");
        Console.WriteLine($"[ERROR] Message: {ex.Message}");

        if (ex.InnerException != null)
        {
            Console.WriteLine($"[ERROR] InnerException: {ex.InnerException.Message}");
        }

        // Print stack trace
        Console.WriteLine($"[ERROR] StackTrace:\n{ex.StackTrace}");

        throw;
    }
}

Console.WriteLine("[INFO] Application starting...");
app.Run();