    using Microsoft.EntityFrameworkCore;
    using JewelChitApplication.Data;
    using JewelChitApplication.Services;
    using JewelChitApplication.Data;
    using JewelChitApplication.Services;

    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);


    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        });

    // Configure PostgreSQL Database
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            npgsqlOptions => npgsqlOptions.CommandTimeout(60)
        ));

    // Register Services
    builder.Services.AddScoped<ICustomerService, CustomerService>();

    // Configure CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAngular", policy =>
        {
            policy.WithOrigins(
                "https://j-chit-frontend.vercel.app/",
               "http://localhost:4200",  // Angular
               "http://localhost:5000",  // Your API HTTP
               "https://localhost:5001"  // Your API HTTPS
           )
                 .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });

    builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
    {
        options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB
    });

// Add Swagger/OpenAPI
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

    // Add logging
    builder.Services.AddLogging(logging =>
    {
        logging.AddConsole();
        logging.AddDebug();
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gold Mortgage API V1");
            c.RoutePrefix = "swagger";
        });
    }
    var uploadsPath = Path.Combine(app.Environment.WebRootPath, "uploads", "company-logos");
    if (!Directory.Exists(uploadsPath))
    {
        Directory.CreateDirectory(uploadsPath);
    }

    app.UseStaticFiles();
    // Enable CORS
    app.UseCors("AllowAngular");

    //app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    // Database Migration and Seeding
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            // Ensure database is created
            context.Database.EnsureCreated();
            // Or use migrations: context.Database.Migrate();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        }
    }

    app.Run();