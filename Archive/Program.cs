// Program.cs

using DSCOVR_Archive;

var builder = WebApplication.CreateBuilder(args);

// Register the background service
builder.Services.AddHostedService<MyBackgroundService>();

builder.Services.AddHttpClient();

var app = builder.Build();

app.MapGet("/", () => "DSCOVR Archive - Md. Jannatul Nayem");

app.Run();
