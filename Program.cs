using Serilog;
using ZenerpSMS.Utils;
using Serilog.Filters;
using Serilog.Formatting.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection.Metadata;
using System;

var builder = WebApplication.CreateBuilder(args);


builder.Host.UseSerilog((context, loggerConfig) =>
{
    var basePath = builder.Configuration["LoggingConfig:LocalLogsPath"] ?? "C:/NATILogs";
    var folderPath = Path.Combine(basePath, DateTime.Now.ToString("yyyy/MMMM"));
    Directory.CreateDirectory(folderPath);
    var smsLogPath = Path.Combine(folderPath, $"{DateTime.Now:dd}.txt");

    loggerConfig
        .ReadFrom.Configuration(context.Configuration) 
        .WriteTo.Console()
        .WriteTo.File(smsLogPath, rollingInterval: RollingInterval.Infinite); 
});

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<LogWriter>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseSerilogRequestLogging(); // ? Add this line

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();



var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application has started successfully.");

app.Run();
