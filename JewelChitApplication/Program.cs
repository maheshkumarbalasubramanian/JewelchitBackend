var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

Console.WriteLine("[INFO] Application building...");
Console.Out.Flush();

app.Run();