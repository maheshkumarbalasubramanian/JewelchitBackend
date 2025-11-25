using Microsoft.EntityFrameworkCore;
using JewelChitApplication.Data;
using JewelChitApplication.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAngular", policy =>
//    {
//        policy.WithOrigins(
//            "https://j-chit-frontend.vercel.app",
//            "http://localhost:4200"
//        )
//        .AllowAnyHeader()
//        .AllowAnyMethod()
//        .AllowCredentials();
//    });
//});

var app = builder.Build();

//app.UseCors("AllowAngular");
app.MapControllers();

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
Console.WriteLine($"[INFO] Starting on port {port}");
Console.Out.Flush();

app.Run($"http://0.0.0.0:{port}");