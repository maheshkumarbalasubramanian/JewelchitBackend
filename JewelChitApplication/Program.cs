using Microsoft.EntityFrameworkCore;
using JewelChitApplication.Data;
using JewelChitApplication.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(
            "https://j-chit-frontend.vercel.app",
            "http://localhost:4200"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors("AllowAngular");
app.MapControllers();

Console.WriteLine("[INFO] Application starting...");
Console.Out.Flush();
app.Run();