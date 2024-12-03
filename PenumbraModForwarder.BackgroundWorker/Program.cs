using PenumbraModForwarder.BackgroundWorker.Extensions;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplicationServices();

// Check if the application is initialized by the Watchdog
bool isInitializedByWatchdog = Environment.GetEnvironmentVariable("WATCHDOG_INITIALIZED") == "true";
Log.Information($"Application initialized by watchdog: {isInitializedByWatchdog}");

if (!isInitializedByWatchdog)
{
    Log.Warning("Application must be started through the main executable");
    return;
}

var host = builder.Build();
host.Run();